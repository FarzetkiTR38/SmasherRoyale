using System.Collections.Generic;
using UnityEngine;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Levels
{
    /// <summary>
    /// Loads and spawns level elements (Stands, Targets, Obstacles) from LevelData ScriptableObjects.
    /// Follows Master Build Prompt Section 9, Section 10, and Phase 19 specifications.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] private GameObject _standPrefab;
        [SerializeField] private GameObject _boxPrefab;
        [SerializeField] private GameObject _stonePrefab;
        [SerializeField] private GameObject _columnPrefab;
        [SerializeField] private GameObject _conePrefab;
        [SerializeField] private GameObject _paintBottlePrefab;

        [Header("Root")]
        [SerializeField] private Transform _levelRoot;

        private readonly List<PhysicsObject> _spawnedObjects = new List<PhysicsObject>();
        private readonly List<GameObject> _spawnedStands = new List<GameObject>();
        private LevelStartPhysicsGate _startPhysicsGate;

        public List<PhysicsObject> SpawnedObjects => _spawnedObjects;
        public List<GameObject> SpawnedStands => _spawnedStands;

        public void BindPrefabs(GameObject stand, GameObject box, GameObject stone, GameObject col, GameObject cone, GameObject bottle)
        {
            _standPrefab = stand;
            _boxPrefab = box;
            _stonePrefab = stone;
            _columnPrefab = col;
            _conePrefab = cone;
            _paintBottlePrefab = bottle;
        }

        public void ClearLevel()
        {
            if (_startPhysicsGate != null) _startPhysicsGate.Release();
            _spawnedObjects.Clear();
            _spawnedStands.Clear();

            // Guarantee any editor showcase preview or level designer working root is completely cleaned up
            var preview = GameObject.Find("SkyCitadelPreview");
            if (preview != null)
            {
                preview.SetActive(false);
                if (Application.isPlaying)
                    Destroy(preview);
                else
                    DestroyImmediate(preview);
            }

            var designerRoot = GameObject.Find("[LevelDesigner_WorkingRoot]");
            if (designerRoot != null)
            {
                designerRoot.SetActive(false);
                if (Application.isPlaying)
                    Destroy(designerRoot);
                else
                    DestroyImmediate(designerRoot);
            }

            // Restore scene stand to active state and default transform
            var existingSceneStand = GameObject.Find("Stand");
            if (existingSceneStand != null)
            {
                existingSceneStand.SetActive(true);
                existingSceneStand.transform.position = new Vector3(0, 0, 9.00f);
                existingSceneStand.transform.localScale = Vector3.one;
                existingSceneStand.transform.rotation = Quaternion.identity;
            }

            if (_levelRoot != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && UnityEditor.Selection.activeTransform != null && UnityEditor.Selection.activeTransform.IsChildOf(_levelRoot))
                {
                    UnityEditor.Selection.activeObject = null;
                }
#endif
                for (int i = _levelRoot.childCount - 1; i >= 0; i--)
                {
                    var child = _levelRoot.GetChild(i).gameObject;
                    if (Application.isPlaying)
                    {
                        // Destroy is deferred until frame end. Retire colliders and targets now,
                        // before the new layout is instantiated into the same positions.
                        child.SetActive(false);
                        Destroy(child);
                    }
                    else
                    {
                        DestroyImmediate(child);
                    }
                }
            }
        }

        public void LoadLevel(LevelData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[LevelLoader] LevelData is null!");
                return;
            }

            ClearLevel();

            if (_levelRoot == null)
            {
                var rootGo = GameObject.Find("Level_ObjectsRoot");
                if (rootGo == null) rootGo = new GameObject("Level_ObjectsRoot");
                _levelRoot = rootGo.transform;
            }

            // 1. Stand setup: Support custom stand scaling and multi-stand setups from LevelData
            var existingSceneStand = GameObject.Find("Stand");
            if (data.Stands != null && data.Stands.Length > 0 && _standPrefab != null)
            {
                if (existingSceneStand != null)
                {
                    existingSceneStand.SetActive(false);
                }

                foreach (var standCfg in data.Stands)
                {
                    var stand = Instantiate(_standPrefab, standCfg.Position, Quaternion.Euler(standCfg.Rotation), _levelRoot);
                    stand.transform.localScale = standCfg.Scale;
                    _spawnedStands.Add(stand);
                }
            }
            else if (existingSceneStand != null)
            {
                existingSceneStand.SetActive(true);
                existingSceneStand.transform.position = new Vector3(0, 0, 9.00f);
                existingSceneStand.transform.localScale = Vector3.one;
                existingSceneStand.transform.rotation = Quaternion.identity;
                _spawnedStands.Add(existingSceneStand);
            }

            // 2. Spawn Target Objects
            if (data.Objects != null)
            {
                foreach (var placement in data.Objects)
                {
                    GameObject prefab = GetPrefabForType(placement.Type);
                    if (prefab == null) continue;

                    var go = Instantiate(prefab, placement.Position, Quaternion.Euler(placement.Rotation), _levelRoot);
                    go.transform.localScale = placement.Scale;

                    if (placement.Type == ObjectType.PaintBottle && placement.PaintColor != Color.clear)
                    {
                        var bottle = go.GetComponent<PaintBottleObject>();
                        if (bottle != null)
                        {
                            bottle.SetColor(placement.PaintColor);
                        }
                    }

                    var physObj = go.GetComponent<PhysicsObject>();
                    if (physObj != null)
                    {
                        physObj.SetRequiredTarget(placement.IsRequiredTarget);
                        if (placement.OverrideMass && physObj.Rigidbody != null)
                        {
                            physObj.Rigidbody.mass = placement.CustomMass;
                        }
                        _spawnedObjects.Add(physObj);
                    }
                }
            }

            if (_startPhysicsGate == null)
                _startPhysicsGate = GetComponent<LevelStartPhysicsGate>() ?? gameObject.AddComponent<LevelStartPhysicsGate>();
            _startPhysicsGate.Prepare(_spawnedObjects, FindFirstObjectByType<MortarController>());

            Debug.Log($"[LevelLoader] Loaded Level {data.LevelId} ('{data.LevelName}') with {_spawnedObjects.Count} targets on {_spawnedStands.Count} stand(s).");
        }

        private GameObject GetPrefabForType(ObjectType type)
        {
            return type switch
            {
                ObjectType.Box => _boxPrefab,
                ObjectType.Stone => _stonePrefab,
                ObjectType.Column => _columnPrefab,
                ObjectType.Cone => _conePrefab,
                ObjectType.PaintBottle => _paintBottlePrefab,
                _ => _boxPrefab
            };
        }
    }
}
