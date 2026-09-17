using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoyalSmash.Skills;

namespace RoyalSmash.VFX
{
    /// <summary>
    /// Central manager for Visual Effects (VFX) in Royal Smash.
    /// Handles particle system pooling, dynamic scaling, and Section 22 event triggers.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        private static VFXManager _instance;
        public static VFXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<VFXManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("VFXManager");
                        _instance = go.AddComponent<VFXManager>();
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuration")]
        [SerializeField] private VFXConfig _config;

        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private Transform _poolRoot;

        public VFXConfig Config
        {
            get
            {
                if (_config == null)
                {
#if UNITY_EDITOR
                    _config = UnityEditor.AssetDatabase.LoadAssetAtPath<VFXConfig>(
                        "Assets/_Project/ScriptableObjects/Config/VFXConfig.asset");
#endif
                    if (_config == null)
                    {
                        _config = ScriptableObject.CreateInstance<VFXConfig>();
                    }
                }
                return _config;
            }
            set => _config = value;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent == null && Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
                InitializePools();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            if (_poolRoot == null)
            {
                var go = new GameObject("VFX_PoolRoot");
                go.transform.SetParent(transform);
                _poolRoot = go.transform;
            }
        }

        private GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            string key = prefab.name;
            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
            }

            GameObject instance = null;
            while (_pools[key].Count > 0 && instance == null)
            {
                instance = _pools[key].Dequeue();
            }

            if (instance == null)
            {
                instance = Instantiate(prefab, position, rotation, _poolRoot);
            }
            else
            {
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.SetActive(true);
            }

            return instance;
        }

        private void ReturnToPool(GameObject instance, string prefabName, float delay)
        {
            if (instance != null)
            {
                StartCoroutine(ReturnRoutine(instance, prefabName, delay));
            }
        }

        private IEnumerator ReturnRoutine(GameObject instance, string key, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (instance != null)
            {
                instance.SetActive(false);
                if (_poolRoot != null) instance.transform.SetParent(_poolRoot);
                if (!_pools.ContainsKey(key)) _pools[key] = new Queue<GameObject>();
                _pools[key].Enqueue(instance);
            }
        }

        // =========================================================================
        // Section 22 VFX Triggers
        // =========================================================================

        /// <summary>
        /// Spawns muzzle blast puff/particles when mortar launches a projectile.
        /// </summary>
        public GameObject SpawnMuzzleFlash(Vector3 position, Quaternion rotation)
        {
            if (Config == null || Config.MuzzleFlashPrefab == null) return null;

            GameObject vfx = GetFromPool(Config.MuzzleFlashPrefab, position, rotation);
            var ps = vfx.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Play();

            ReturnToPool(vfx, Config.MuzzleFlashPrefab.name, 1.0f);
            return vfx;
        }

        /// <summary>
        /// Spawns collision sparks/dust scaling with impulse.
        /// </summary>
        public GameObject SpawnImpact(Vector3 position, Vector3 normal, float impulse)
        {
            if (Config == null) return null;

            bool isHeavy = impulse >= Config.HeavyImpactThreshold;
            GameObject prefab = isHeavy ? Config.ImpactHeavyPrefab : Config.ImpactGenericPrefab;
            if (prefab == null) prefab = Config.ImpactGenericPrefab;
            if (prefab == null) return null;

            Quaternion rotation = normal != Vector3.zero ? Quaternion.LookRotation(normal) : Quaternion.identity;
            GameObject vfx = GetFromPool(prefab, position, rotation);

            var ps = vfx.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startSizeMultiplier = Mathf.Clamp(0.8f + (impulse * 0.15f), 0.8f, 2.5f);
                ps.Play();
            }

            ReturnToPool(vfx, prefab.name, 1.2f);
            return vfx;
        }

        /// <summary>
        /// Spawns crumbling debris fragments when a target object is cleared.
        /// </summary>
        public GameObject SpawnDestruction(Vector3 position, Color objectColor)
        {
            if (Config == null || Config.ObjectDestructionPrefab == null) return null;

            GameObject vfx = GetFromPool(Config.ObjectDestructionPrefab, position, Quaternion.identity);
            var ps = vfx.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = objectColor;
                ps.Play();
            }

            ReturnToPool(vfx, Config.ObjectDestructionPrefab.name, 1.5f);
            return vfx;
        }

        /// <summary>
        /// Spawns glass shards and lingering paint spill for PaintBottle shatter.
        /// </summary>
        public void SpawnBottleBreak(Vector3 position, Color paintColor)
        {
            if (Config == null) return;

            // 1. Glass shards burst
            if (Config.BottleBreakShardsPrefab != null)
            {
                GameObject shards = GetFromPool(Config.BottleBreakShardsPrefab, position, Quaternion.identity);
                var ps = shards.GetComponentInChildren<ParticleSystem>();
                if (ps != null) ps.Play();
                ReturnToPool(shards, Config.BottleBreakShardsPrefab.name, 1.5f);
            }

            // 2. Lingering paint spill puddle
            if (Config.PaintSpillPrefab != null)
            {
                Vector3 puddlePos = new Vector3(position.x, 0.05f, position.z);
                GameObject spill = GetFromPool(Config.PaintSpillPrefab, puddlePos, Quaternion.Euler(90f, 0f, 0f));
                var renderer = spill.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    if (Application.isPlaying)
                    {
                        renderer.material.color = paintColor;
                    }
                    else if (renderer.sharedMaterial != null)
                    {
                        renderer.sharedMaterial.color = paintColor;
                    }
                }
                ReturnToPool(spill, Config.PaintSpillPrefab.name, Config.PaintSpillLifetime);
            }
        }

        /// <summary>
        /// Spawns pulse flash/aura upon skill activation.
        /// </summary>
        public GameObject SpawnSkillActivation(Vector3 worldPos, SkillType type)
        {
            if (Config == null || Config.SkillActivatePrefab == null) return null;

            GameObject vfx = GetFromPool(Config.SkillActivatePrefab, worldPos, Quaternion.identity);
            var ps = vfx.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = type == SkillType.MegaBall
                    ? new Color(1.0f, 0.45f, 0.0f, 1f) // Fiery Orange
                    : new Color(0.0f, 0.85f, 1.0f, 1f); // Electric Cyan
                ps.Play();
            }

            ReturnToPool(vfx, Config.SkillActivatePrefab.name, 1.5f);
            return vfx;
        }

        /// <summary>
        /// Spawns celebratory victory confetti at target location.
        /// </summary>
        public GameObject SpawnWinCelebration(Vector3 position)
        {
            if (Config == null || Config.WinConfettiPrefab == null) return null;

            GameObject vfx = GetFromPool(Config.WinConfettiPrefab, position, Quaternion.identity);
            var ps = vfx.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Play();

            ReturnToPool(vfx, Config.WinConfettiPrefab.name, 3.0f);
            return vfx;
        }

        /// <summary>
        /// Triggers a burst of animated UI coins flying towards the gold counter.
        /// </summary>
        public void PlayRewardCoins(Vector3 startScreenPos, Vector3 targetScreenPos, int count = 10, Action onComplete = null)
        {
            if (Config == null || Config.CoinFlyPrefab == null)
            {
                onComplete?.Invoke();
                return;
            }

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                onComplete?.Invoke();
                return;
            }

            int completedCount = 0;
            for (int i = 0; i < count; i++)
            {
                GameObject coin = Instantiate(Config.CoinFlyPrefab, canvas.transform);
                var flyAnim = coin.GetComponent<CoinFlyAnimation>();
                if (flyAnim == null) flyAnim = coin.AddComponent<CoinFlyAnimation>();

                float delay = i * 0.06f;
                flyAnim.AnimateFlight(startScreenPos, targetScreenPos, delay, () =>
                {
                    completedCount++;
                    if (completedCount >= count)
                    {
                        onComplete?.Invoke();
                    }
                });
            }
        }
    }
}
