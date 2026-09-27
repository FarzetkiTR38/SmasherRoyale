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

            // Guarantee any editor showcase preview is completely cleaned up
            var preview = GameObject.Find("SkyCitadelPreview");
            if (preview != null)
            {
                preview.SetActive(false);
                if (Application.isPlaying)
                    Destroy(preview);
                else
                    DestroyImmediate(preview);
            }

            if (_levelRoot != null)
            {
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

            // 1. Stand setup: Use existing scene stand if present, otherwise spawn from prefab
            var existingSceneStand = GameObject.Find("Stand");
            if (existingSceneStand != null)
            {
                // The scene already has the Royal Stand platform (at Z=7.50)
                _spawnedStands.Add(existingSceneStand);
            }
            else if (data.Stands != null && _standPrefab != null)
            {
                foreach (var standCfg in data.Stands)
                {
                    var stand = Instantiate(_standPrefab, standCfg.Position, Quaternion.Euler(standCfg.Rotation), _levelRoot);
                    stand.transform.localScale = standCfg.Scale;
                    _spawnedStands.Add(stand);
                }
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
