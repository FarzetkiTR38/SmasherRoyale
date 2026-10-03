// Changes the two existing metal materials while retaining identity and legacy settings.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace RoyalSmash.Editor
{
    /// <summary>Changes only the shader/polish values of the two existing opaque metal materials.</summary>
    public static class Phase32MetalPolishRevision
    {
        private const string ShaderName = "RoyalSmash/Citadel Polished Metal";
        private const string Art = "Assets/_Project/Art/SkyCitadel";
        private static readonly string[] MaterialPaths = {
            Art + "/Materials/M_AntiqueGold.mat", Art + "/Materials/M_RoyalSapphire.mat"
        };
        private static readonly HashSet<string> SupportedKeywords = new HashSet<string> {
            "_ALPHATEST_ON", "_EMISSION", "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
            "_SPECULARHIGHLIGHTS_OFF", "_ENVIRONMENTREFLECTIONS_OFF", "_RECEIVE_SHADOWS_OFF"
        };
        private static readonly string[] UnsupportedMaps = {
            "_BumpMap", "_MetallicGlossMap", "_SpecGlossMap", "_OcclusionMap", "_ParallaxMap",
            "_DetailMask", "_DetailAlbedoMap", "_DetailNormalMap", "_ClearCoatMap"
        };
        private static readonly string[] PreserveFloats = {
            "_Metallic", "_Smoothness", "_Cutoff", "_WorkflowMode", "_Surface", "_AlphaClip",
            "_Cull", "_ZWrite", "_ReceiveShadows", "_SpecularHighlights", "_EnvironmentReflections",
            "_SmoothnessTextureChannel", "_SrcBlend", "_DstBlend", "_SrcBlendAlpha", "_DstBlendAlpha", "_AlphaToMask"
        };
        private static readonly string[] PreserveColors = { "_BaseColor", "_EmissionColor" };

        [Serializable] public sealed class BackupEntry
        {
            public string assetPath, guid, materialSha256, metaSha256, backupFile;
        }
        [Serializable] public sealed class BackupManifest
        {
            public string shaderName;
            public BackupEntry[] entries;
        }
        private static string Workspace => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        private static string BackupRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex/local-backups/Phase32MetalPolish");

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/12 - Apply Metal Polish")]
        public static void ApplyMenu() => Apply();

        // Returns a byte-exact rollback folder, or null when the preset is already applied.
        public static string Apply()
        {
            RequireEditMode();
            Shader shader = Shader.Find(ShaderName);
            if (shader == null || !shader.isSupported || ShaderUtil.ShaderHasError(shader))
                throw new InvalidOperationException("Import and successfully compile the metal shader before applying.");
            Material[] materials = MaterialPaths.Select(AssetDatabase.LoadAssetAtPath<Material>).ToArray();
            foreach (Material material in materials) Validate(material);
            if (materials.Select((m, i) => MatchesPreset(m, shader, i)).All(v => v)) return null;

            // Refuse unsaved material edits: the disk backup must represent the in-memory baseline.
            foreach (Material material in materials)
                if (EditorUtility.IsDirty(material))
                    throw new InvalidOperationException("Save or preserve the current material edits before applying: " + material.name);
            string backupDirectory = Path.Combine(BackupRoot, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(backupDirectory);
            var manifest = new BackupManifest { shaderName = ShaderName, entries = new BackupEntry[materials.Length] };
            for (int i = 0; i < materials.Length; i++)
            {
                string file = Path.Combine(Workspace, MaterialPaths[i]);
                string backupFile = i + "-" + Path.GetFileName(file);
                byte[] original = File.ReadAllBytes(file);
                File.WriteAllBytes(Path.Combine(backupDirectory, backupFile), original);
                manifest.entries[i] = new BackupEntry {
                    assetPath = MaterialPaths[i], guid = AssetDatabase.AssetPathToGUID(MaterialPaths[i]),
                    materialSha256 = Hash(original), metaSha256 = Hash(File.ReadAllBytes(file + ".meta")), backupFile = backupFile
                };
            }
            File.WriteAllText(Path.Combine(backupDirectory, "manifest.json"), JsonUtility.ToJson(manifest, true));
            Material[] originals = materials.Select(m => new Material(m) { hideFlags = HideFlags.HideAndDontSave }).ToArray();
            try
            {
                Undo.RecordObjects(materials, "Phase32 polished metal materials");
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    string[] keywords = material.shaderKeywords.ToArray();
                    material.shader = shader;
                    material.shaderKeywords = keywords; // Never clear a legitimate existing keyword.
                    SetPreset(material, i);
                    AssertLegacyPreserved(originals[i], material);
                    EditorUtility.SetDirty(material);
                    AssetDatabase.SaveAssetIfDirty(material);
                    if (AssetDatabase.AssetPathToGUID(MaterialPaths[i]) != manifest.entries[i].guid ||
                        Hash(File.ReadAllBytes(Path.Combine(Workspace, MaterialPaths[i]) + ".meta")) != manifest.entries[i].metaSha256)
                        throw new InvalidOperationException("Material identity changed: " + MaterialPaths[i]);
                }
            }
            catch (Exception applyError)
            {
                // Restore every original even when the failure happens between the two materials.
                var rollbackErrors = new List<Exception>();
                for (int i = 0; i < materials.Length; i++)
                {
                    try { EditorUtility.CopySerialized(originals[i], materials[i]); }
                    catch (Exception error) { rollbackErrors.Add(new InvalidOperationException("In-memory rollback failed: " + MaterialPaths[i], error)); }
                }
                try { RestoreFiles(backupDirectory, manifest); }
                catch (Exception error) { rollbackErrors.Add(error); }
                if (rollbackErrors.Count == 0) throw;
                rollbackErrors.Insert(0, applyError);
                throw new AggregateException("Metal apply failed; every material rollback was attempted. Backup: " + backupDirectory, rollbackErrors);
            }
            finally { foreach (Material original in originals) UnityEngine.Object.DestroyImmediate(original); }
            Debug.Log("[Phase32MetalPolishRevision] Two existing materials updated. Rollback: " + backupDirectory);
            return backupDirectory;
        }

        public static void Rollback(string backupDirectory)
        {
            RequireEditMode();
            string resolved = Path.GetFullPath(backupDirectory);
            string allowedRoot = Path.GetFullPath(BackupRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!resolved.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Rollback folder must be inside the user profile's Phase32 metal backup directory.");
            var manifest = JsonUtility.FromJson<BackupManifest>(File.ReadAllText(Path.Combine(resolved, "manifest.json")));
            if (manifest == null || manifest.shaderName != ShaderName || manifest.entries == null ||
                manifest.entries.Length != MaterialPaths.Length ||
                !manifest.entries.Select(e => e.assetPath).OrderBy(p => p).SequenceEqual(MaterialPaths.OrderBy(p => p)))
                throw new InvalidOperationException("Unexpected metal rollback manifest.");
            foreach (BackupEntry entry in manifest.entries)
            {
                string file = Path.Combine(Workspace, entry.assetPath);
                if (AssetDatabase.AssetPathToGUID(entry.assetPath) != entry.guid ||
                    Hash(File.ReadAllBytes(file + ".meta")) != entry.metaSha256 ||
                    Path.GetFileName(entry.backupFile) != entry.backupFile ||
                    Hash(File.ReadAllBytes(Path.Combine(resolved, entry.backupFile))) != entry.materialSha256)
                    throw new InvalidOperationException("Rollback identity/bytes do not match: " + entry.assetPath);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(entry.assetPath);
                if (material != null && EditorUtility.IsDirty(material))
                    throw new InvalidOperationException("Preserve unsaved material changes before rollback: " + entry.assetPath);
            }
            RestoreFiles(resolved, manifest);
            Debug.Log("[Phase32MetalPolishRevision] Original material bytes restored; GUIDs retained.");
        }

        private static void RestoreFiles(string directory, BackupManifest manifest)
        {
            // Write the complete pair before reimporting, so a reload cannot see a mixed pair.
            var errors = new List<Exception>();
            var written = new HashSet<string>();
            foreach (BackupEntry entry in manifest.entries)
            {
                try
                {
                    File.WriteAllBytes(Path.Combine(Workspace, entry.assetPath), File.ReadAllBytes(Path.Combine(directory, entry.backupFile)));
                    written.Add(entry.assetPath);
                }
                catch (Exception error) { errors.Add(new IOException("Restore write failed: " + entry.assetPath, error)); }
            }
            foreach (BackupEntry entry in manifest.entries)
            {
                if (!written.Contains(entry.assetPath)) continue;
                try
                {
                    AssetDatabase.ImportAsset(entry.assetPath, ImportAssetOptions.ForceUpdate);
                    string restoredPath = Path.Combine(Workspace, entry.assetPath);
                    if (AssetDatabase.AssetPathToGUID(entry.assetPath) != entry.guid ||
                        Hash(File.ReadAllBytes(restoredPath)) != entry.materialSha256 ||
                        Hash(File.ReadAllBytes(restoredPath + ".meta")) != entry.metaSha256)
                        throw new InvalidOperationException("Rollback byte/GUID/meta verification failed: " + entry.assetPath);
                }
                catch (Exception error) { errors.Add(new InvalidOperationException("Restore import/verification failed: " + entry.assetPath, error)); }
            }
            if (errors.Count > 0) throw new AggregateException("Every metal rollback entry was attempted.", errors);
        }

        private static void Validate(Material material)
        {
            if (material == null || material.shader == null ||
                (material.shader.name != "Universal Render Pipeline/Lit" && material.shader.name != ShaderName))
                throw new InvalidOperationException("Only the existing Lit/polished gold and sapphire materials are supported.");
            if (material.HasProperty("_Surface") && material.GetFloat("_Surface") != 0 ||
                material.HasProperty("_WorkflowMode") && material.GetFloat("_WorkflowMode") != 1 ||
                material.HasProperty("_ClearCoatMask") && material.GetFloat("_ClearCoatMask") > 0 ||
                material.HasProperty("_ZWrite") && material.GetFloat("_ZWrite") != 1 ||
                material.HasProperty("_SrcBlend") && material.GetFloat("_SrcBlend") != 1 ||
                material.HasProperty("_DstBlend") && material.GetFloat("_DstBlend") != 0)
                throw new InvalidOperationException("Opaque metallic Lit state is required: " + material.name);
            foreach (string keyword in material.shaderKeywords)
                if (!SupportedKeywords.Contains(keyword))
                    throw new InvalidOperationException("Unsupported active keyword: " + material.name + " / " + keyword);
            // Conservative: an assigned unsupported map is rejected even if its keyword is stale/off.
            foreach (string property in UnsupportedMaps)
                if (material.HasProperty(property) && material.GetTexture(property) != null)
                    throw new InvalidOperationException("Unsupported assigned map: " + material.name + " / " + property);
            foreach (string property in new[] { "_BaseMap", "_BaseColor", "_EmissionColor", "_Metallic", "_Smoothness", "_Cutoff" })
                if (!material.HasProperty(property)) throw new InvalidOperationException("Required legacy property missing: " + property);
        }

        private static void SetPreset(Material material, int index)
        {
            bool gold = index == 0;
            material.SetFloat("_ShadeStrength", gold ? 1f : .90f);
            material.SetVector("_ShadeTint", gold ? new Vector4(.18f,.095f,.025f,0) : new Vector4(.10f,.17f,.35f,0));
            material.SetColor("_HighlightTint", gold ? new Color(1,.92f,.67f,1) : new Color(.60f,.83f,1f,1));
            material.SetFloat("_FaceContrast", gold ? .82f : .90f);
            material.SetFloat("_HighlightBlend", gold ? .58f : .78f);
            material.SetVector("_PolishAxisOS", new Vector4(.82f,.57f,.05f,0));
            material.SetFloat("_PolishFrequency", .58f);
            material.SetFloat("_PolishPhase", gold ? .35f : .78f);
            material.SetFloat("_PolishNormalBend", gold ? .075f : .055f);
            material.SetFloat("_PolishSpecular", gold ? .55f : .40f);
            material.SetFloat("_StripWidth", .42f);
            material.SetFloat("_CurvatureStrength", gold ? .075f : .05f);
            // Existing _BaseColor/_Metallic/_Smoothness/maps/render state are deliberately untouched.
        }

        private static bool MatchesPreset(Material material, Shader shader, int index)
        {
            if (material.shader != shader) return false;
            var expected = new Material(material) { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                SetPreset(expected, index);
                return new[] { "_ShadeStrength", "_FaceContrast", "_HighlightBlend", "_PolishFrequency", "_PolishPhase", "_PolishNormalBend", "_PolishSpecular", "_StripWidth", "_CurvatureStrength" }
                    .All(p => Mathf.Approximately(material.GetFloat(p), expected.GetFloat(p))) &&
                    material.GetVector("_ShadeTint") == expected.GetVector("_ShadeTint") &&
                    material.GetVector("_PolishAxisOS") == expected.GetVector("_PolishAxisOS") &&
                    material.GetColor("_HighlightTint") == expected.GetColor("_HighlightTint");
            }
            finally { UnityEngine.Object.DestroyImmediate(expected); }
        }

        private static void AssertLegacyPreserved(Material before, Material after)
        {
            foreach (string property in PreserveFloats)
                if (before.HasProperty(property) && (!after.HasProperty(property) || before.GetFloat(property) != after.GetFloat(property)))
                    throw new InvalidOperationException("Legacy numeric property changed: " + property);
            foreach (string property in PreserveColors)
                if (before.GetColor(property) != after.GetColor(property))
                    throw new InvalidOperationException("Legacy color changed: " + property);
            foreach (string property in new[] { "_BaseMap", "_EmissionMap" })
                if (before.GetTexture(property) != after.GetTexture(property) ||
                    before.GetTextureScale(property) != after.GetTextureScale(property) ||
                    before.GetTextureOffset(property) != after.GetTextureOffset(property))
                    throw new InvalidOperationException("Legacy texture binding changed: " + property);
            if (!before.shaderKeywords.OrderBy(k => k).SequenceEqual(after.shaderKeywords.OrderBy(k => k)) ||
                before.renderQueue != after.renderQueue || before.enableInstancing != after.enableInstancing ||
                before.doubleSidedGI != after.doubleSidedGI || before.globalIlluminationFlags != after.globalIlluminationFlags ||
                before.GetTag("RenderType", false) != after.GetTag("RenderType", false))
                throw new InvalidOperationException("Legacy keyword/render state changed.");
            foreach (string pass in new[] { "ShadowCaster", "DepthOnly", "MOTIONVECTORS" })
                if (before.GetShaderPassEnabled(pass) != after.GetShaderPassEnabled(pass))
                    throw new InvalidOperationException("Legacy pass state changed: " + pass);
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                throw new InvalidOperationException("Apply/rollback require a settled Edit Mode editor.");
        }
        private static string Hash(byte[] bytes)
        {
            using (var algorithm = SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }
}
