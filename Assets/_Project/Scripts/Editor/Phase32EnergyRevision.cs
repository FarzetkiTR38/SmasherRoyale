using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace RoyalSmash.Editor
{
    /// <summary>Revises the shared energy material in place; no prefab, scene or gameplay mutation.</summary>
    public static class Phase32EnergyRevision
    {
        private const string MATERIAL = "Assets/_Project/Art/SkyCitadel/Materials/M_ArcaneEnergy.mat";
        private const string SHADER = "Assets/_Project/Art/SkyCitadel/Shaders/CitadelEnergyReference.shader";
        private const string REPORT = "_FarzetkiDocs/Art/Phase32/32-08-energy-material.json";
        private const float COLOR_TOLERANCE = 1e-6f;
        private static readonly string[] COLORS = { "_BaseColor", "_GlowColor", "_CoreColor" };
        private static readonly string[] FLOATS = { "_NoiseScale", "_FlowSpeed", "_RimStrength", "_SpecularStrength" };

        [Serializable] public sealed class ColorValue
        {
            public string property;
            public Color value, valueAfter;
            public float maxChannelDifference;
        }
        [Serializable] public sealed class FloatValue { public string property; public float value; }
        [Serializable] public sealed class TextureValue
        {
            public string property, assetPath;
            public Vector2 scale, offset;
        }
        [Serializable] public sealed class RevisionReport
        {
            public string appliedUtc, materialPath, materialGuid, shaderBefore, shaderAfter;
            public bool guidPreserved, legacyValuesPreserved, textureBindingsPreserved;
            public float colorPreservationTolerance, maxColorChannelDifference;
            public string status = "applied; runtime and visual verification pending";
            public string scope = "Existing M_ArcaneEnergy shader only; new shader defaults add filament controls. No scene, prefab or runtime code changed.";
            public string budget = "2 procedural 3D value-noise evaluations / fragment, 0 material texture fetches, main-light shadow sampling; no transparency or ray march. Forward, ShadowCaster and DepthOnly passes.";
            public string runtimeColor = "MainColor attribute binds Material.color to _BaseColor; non-blue runtime tints also recolor emission. Opaque shrink behavior retained.";
            public List<ColorValue> colors = new List<ColorValue>();
            public List<FloatValue> floats = new List<FloatValue>();
            public List<TextureValue> textures = new List<TextureValue>();
        }
        public static RevisionReport LastReport { get; private set; }

        [MenuItem("Royal Smash/Sky Citadel/Phase 32/08 - Apply Energy Surface Revision")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainGame.unity")
                throw new InvalidOperationException("Open MainGame in Edit Mode before revising the shared energy material.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(SHADER);
            if (material == null || shader == null || !shader.isSupported)
                throw new InvalidOperationException("Existing energy material and supported reference shader are required.");
            if (material.shader == null || (material.shader.name != "RoyalSmash/Citadel Energy" && material.shader != shader))
                throw new InvalidOperationException("Unexpected energy shader; preserve the current custom material for review.");
            foreach (var message in ShaderUtil.GetShaderMessages(shader))
                if (message.severity.ToString() == "Error") throw new InvalidOperationException(message.message);

            var report = new RevisionReport { appliedUtc = DateTime.UtcNow.ToString("O"), materialPath = MATERIAL,
                materialGuid = AssetDatabase.AssetPathToGUID(MATERIAL), shaderBefore = material.shader.name,
                shaderAfter = shader.name, colorPreservationTolerance = COLOR_TOLERANCE };
            foreach (string property in COLORS)
            {
                if (!material.HasProperty(property)) throw new InvalidOperationException("Missing legacy energy property " + property);
                report.colors.Add(new ColorValue { property = property, value = material.GetColor(property) });
            }
            foreach (string property in FLOATS)
            {
                if (!material.HasProperty(property)) throw new InvalidOperationException("Missing legacy energy property " + property);
                report.floats.Add(new FloatValue { property = property, value = material.GetFloat(property) });
            }
            // The established procedural material has no maps. Refuse an unexpected map-bearing
            // variation rather than silently disconnecting its authored texture properties.
            foreach (string property in material.GetTexturePropertyNames())
            {
                var texture = material.GetTexture(property);
                report.textures.Add(new TextureValue { property = property, assetPath = AssetDatabase.GetAssetPath(texture),
                    scale = material.GetTextureScale(property), offset = material.GetTextureOffset(property) });
            }
            if (report.textures.Count != 0)
                throw new InvalidOperationException("Energy material now has texture properties; review their shader bindings before switching.");

            // Clone the complete serialized material, including authored properties that
            // are not exposed by this tool. Do not copy temporary HideAndDontSave flags
            // onto the persistent asset during rollback.
            var backup = new Material(material);
            EditorUtility.CopySerialized(material, backup);
            try
            {
                Undo.RecordObject(material, "Phase32 energy surface revision");
                material.shader = shader;
                foreach (var value in report.colors) material.SetColor(value.property, value.value);
                foreach (var value in report.floats) material.SetFloat(value.property, value.value);
                report.legacyValuesPreserved = true;
                foreach (var value in report.colors)
                {
                    value.valueAfter = material.GetColor(value.property);
                    value.maxChannelDifference = ColorDifference(value.value, value.valueAfter);
                    report.maxColorChannelDifference = Mathf.Max(report.maxColorChannelDifference, value.maxChannelDifference);
                    // MainColor storage can round-trip by a few float ULPs. Bound each
                    // channel tightly instead of accepting an approximate Color operator.
                    report.legacyValuesPreserved &= value.maxChannelDifference <= COLOR_TOLERANCE;
                }
                foreach (var value in report.floats)
                    report.legacyValuesPreserved &= material.GetFloat(value.property).Equals(value.value);
                report.guidPreserved = AssetDatabase.AssetPathToGUID(MATERIAL) == report.materialGuid;
                report.textureBindingsPreserved = material.GetTexturePropertyNames().Length == 0;
                if (!report.legacyValuesPreserved || !report.guidPreserved || !report.textureBindingsPreserved)
                    throw new InvalidOperationException("Energy material preservation check failed; restoring the original material.");
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
                WriteReport(report);
                LastReport = report;
            }
            catch (Exception revisionError)
            {
                try
                {
                    EditorUtility.CopySerialized(backup, material);
                    EditorUtility.SetDirty(material);
                    AssetDatabase.SaveAssetIfDirty(material);
                }
                catch (Exception restorationError)
                {
                    throw new AggregateException("Energy revision failed and the material backup could not be fully restored.",
                        revisionError, restorationError);
                }
                throw;
            }
            finally { UnityEngine.Object.DestroyImmediate(backup); }
            Debug.Log("[Phase32EnergyRevision] " + JsonUtility.ToJson(report));
        }

        private static float ColorDifference(Color before, Color after)
        {
            float maximum = 0;
            for (int channel = 0; channel < 4; channel++)
            {
                float difference = Mathf.Abs(before[channel] - after[channel]);
                if (float.IsNaN(difference) || float.IsInfinity(difference)) return float.PositiveInfinity;
                maximum = Mathf.Max(maximum, difference);
            }
            return maximum;
        }

        private static void WriteReport(RevisionReport report)
        {
            string temporary = REPORT + ".tmp";
            Directory.CreateDirectory(Path.GetDirectoryName(REPORT));
            try
            {
                File.WriteAllText(temporary, JsonUtility.ToJson(report, true));
                if (File.Exists(REPORT)) File.Replace(temporary, REPORT, null);
                else File.Move(temporary, REPORT);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }
    }
}
