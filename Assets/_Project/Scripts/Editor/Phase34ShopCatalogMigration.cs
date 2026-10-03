#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RoyalSmash.Economy;
using RoyalSmash.UI;

namespace RoyalSmash.EditorTools
{
    /// <summary>
    /// Binds the eight existing MainMenu purchase buttons without touching economic data,
    /// layout, legacy inline data, scene loading, or scene saving. Root takes byte backups
    /// before applying and saves the reviewed MainMenu scene separately.
    /// </summary>
    public static class Phase34ShopCatalogMigration
    {
        public const string ScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        public const string CatalogPath = "Assets/_Project/ScriptableObjects/Config/ShopConfig.asset";

        private static readonly string[] ExpectedIds =
        {
            "deal_kings_royal", "no_ads_pass", "gold_pouch_800", "gold_chest_1600",
            "skill_mega_5", "skill_infinity_5", "bundle_warrior", "bundle_champion"
        };

        [Serializable]
        public sealed class BindingResult
        {
            public string hierarchyPath;
            public string stableItemId;
            public string productId;
            public bool changed;
        }

        [Serializable]
        public sealed class MigrationReport
        {
            public string capturedUtc;
            public string scenePath;
            public string catalogPath;
            public int catalogItemCount;
            public int cardCount;
            public int changedCount;
            public bool catalogUnchanged;
            public bool sceneWasDirty;
            public bool sceneSaved;
            public BindingResult[] bindings;
        }

        private sealed class PendingBinding
        {
            public ShopItemCardUI Card;
            public ShopItemData Product;
            public UnityEngine.Object PreviousCatalog;
            public string PreviousId;
            public bool Changed;
        }

        public static MigrationReport ApplyToOpenMainMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Shop catalog migration requires Edit Mode.");

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("Open MainMenu before migration; this helper never loads or saves scenes.");

            ShopConfig catalog = AssetDatabase.LoadAssetAtPath<ShopConfig>(CatalogPath);
            if (catalog == null) throw new InvalidOperationException("The current ShopConfig asset is missing.");
            string catalogBefore = EditorJsonUtility.ToJson(catalog, false);
            Dictionary<string, ShopItemData> products = ValidateCatalog(catalog);

            Transform shopRoot = FindShopRoot(scene);
            if (shopRoot == null) throw new InvalidOperationException("MainMenuCanvas/PagesRoot/ShopPage was not found.");
            ShopItemCardUI[] cards = shopRoot.GetComponentsInChildren<ShopItemCardUI>(true);
            if (cards.Length != ExpectedIds.Length)
                throw new InvalidOperationException("Expected exactly eight existing ShopItemCardUI purchase buttons.");

            // Preflight all cards before mutating even the first serialized field.
            var pending = new List<PendingBinding>();
            var foundIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (ShopItemCardUI card in cards)
            {
                if (card.gameObject.scene != scene || card.GetComponent<Button>() == null)
                    throw new InvalidOperationException("A shop card is not an existing MainMenu purchase button.");

                var serialized = new SerializedObject(card);
                SerializedProperty legacy = RequireProperty(serialized, "_itemData");
                SerializedProperty legacyId = legacy.FindPropertyRelative("Id");
                if (legacyId == null || string.IsNullOrWhiteSpace(legacyId.stringValue))
                    throw new InvalidOperationException("A shop button has no legacy item Id for migration.");

                string id = legacyId.stringValue;
                ShopItemData product;
                if (!products.TryGetValue(id, out product) || !foundIds.Add(id))
                    throw new InvalidOperationException("Unexpected or duplicate legacy shop Id: " + id);

                SerializedProperty catalogProperty = RequireProperty(serialized, "_catalog");
                SerializedProperty idProperty = RequireProperty(serialized, "_catalogItemId");
                UnityEngine.Object oldCatalog = catalogProperty.objectReferenceValue;
                string oldId = idProperty.stringValue;
                bool unbound = oldCatalog == null && string.IsNullOrEmpty(oldId);
                bool alreadyBound = oldCatalog == catalog && string.Equals(oldId, id, StringComparison.Ordinal);
                if (!unbound && !alreadyBound)
                    throw new InvalidOperationException("Existing custom catalog binding must be reviewed: " + GetPath(card.transform));

                pending.Add(new PendingBinding
                {
                    Card = card,
                    Product = product,
                    PreviousCatalog = oldCatalog,
                    PreviousId = oldId,
                    Changed = !alreadyBound
                });
            }

            foreach (string expected in ExpectedIds)
                if (!foundIds.Contains(expected)) throw new InvalidOperationException("Missing existing card: " + expected);

            var report = new MigrationReport
            {
                capturedUtc = DateTime.UtcNow.ToString("o"),
                scenePath = scene.path,
                catalogPath = CatalogPath,
                catalogItemCount = products.Count,
                cardCount = cards.Length,
                sceneWasDirty = scene.isDirty,
                sceneSaved = false,
                bindings = new BindingResult[cards.Length]
            };

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Phase 34: Bind existing shop cards to catalog");
            try
            {
                for (int i = 0; i < pending.Count; i++)
                {
                    PendingBinding binding = pending[i];
                    if (binding.Changed)
                    {
                        Undo.RecordObject(binding.Card, "Bind shop catalog Id");
                        var serialized = new SerializedObject(binding.Card);
                        RequireProperty(serialized, "_catalog").objectReferenceValue = catalog;
                        RequireProperty(serialized, "_catalogItemId").stringValue = binding.Product.Id;
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(binding.Card);
                        if (!ReferenceEquals(binding.Card.ItemData, binding.Product))
                            throw new InvalidOperationException("The live catalog binding did not resolve: " + binding.Product.Id);
                        report.changedCount++;
                    }

                    report.bindings[i] = new BindingResult
                    {
                        hierarchyPath = GetPath(binding.Card.transform),
                        stableItemId = binding.Product.Id,
                        productId = binding.Product.ProductId,
                        changed = binding.Changed
                    };
                }

                report.catalogUnchanged = string.Equals(catalogBefore, EditorJsonUtility.ToJson(catalog, false), StringComparison.Ordinal);
                if (!report.catalogUnchanged)
                    throw new InvalidOperationException("Catalog data changed unexpectedly; abort this migration.");
                if (report.changedCount > 0) EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(undoGroup);
                return report;
            }
            catch
            {
                // Revert just our two binding fields. Root still retains an independent
                // scene/catalog byte backup and decides whether to save after review.
                foreach (PendingBinding binding in pending)
                {
                    var serialized = new SerializedObject(binding.Card);
                    RequireProperty(serialized, "_catalog").objectReferenceValue = binding.PreviousCatalog;
                    RequireProperty(serialized, "_catalogItemId").stringValue = binding.PreviousId;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                Undo.CollapseUndoOperations(undoGroup);
                throw;
            }
        }

        private static Dictionary<string, ShopItemData> ValidateCatalog(ShopConfig catalog)
        {
            List<ShopItemData> items = catalog.GetAllItems();
            if (items.Count != ExpectedIds.Length)
                throw new InvalidOperationException("Expected exactly eight current catalog products.");

            var products = new Dictionary<string, ShopItemData>(StringComparer.Ordinal);
            var skus = new HashSet<string>(StringComparer.Ordinal);
            foreach (ShopItemData item in items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.ProductId))
                    throw new InvalidOperationException("A current catalog item has an invalid Id or ProductId.");
                if (products.ContainsKey(item.Id) || !skus.Add(item.ProductId))
                    throw new InvalidOperationException("Duplicate current catalog item Id or ProductId.");
                products.Add(item.Id, item);
            }
            foreach (string expected in ExpectedIds)
                if (!products.ContainsKey(expected)) throw new InvalidOperationException("Missing current catalog product: " + expected);
            foreach (KeyValuePair<string, ShopItemData> entry in products)
                if (!ReferenceEquals(catalog.FindItemById(entry.Key), entry.Value))
                    throw new InvalidOperationException("A billing SKU aliases another stable catalog Id: " + entry.Key);
            return products;
        }

        private static SerializedProperty RequireProperty(SerializedObject serialized, string name)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null) throw new InvalidOperationException("Missing ShopItemCardUI serialized field: " + name);
            return property;
        }

        private static Transform FindShopRoot(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != "MainMenuCanvas") continue;
                return root.transform.Find("PagesRoot/ShopPage");
            }
            return null;
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}
#endif
