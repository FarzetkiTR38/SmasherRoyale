using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Interactive controller for the Physics Test Playground scene.
    /// Tracks initial object transforms and provides instant reset functionality (via 'R' key or UI button).
    /// Gives the player plenty of ammo to freely experiment with physics interactions.
    /// </summary>
    public class PlaygroundController : MonoBehaviour
    {
        [System.Serializable]
        public struct ObjectSpawnInfo
        {
            public GameObject Prefab;
            public Vector3 Position;
            public Quaternion Rotation;
        }

        [Header("References")]
        [SerializeField] private MortarController _mortar;
        [SerializeField] private Button _resetButton;
        [SerializeField] private TextMeshProUGUI _infoText;

        [Header("Spawn Configuration")]
        [SerializeField] private List<ObjectSpawnInfo> _spawnList = new List<ObjectSpawnInfo>();
        [SerializeField] private int _playgroundAmmo = 99;

        private readonly List<GameObject> _activeObjects = new List<GameObject>();

        private void Start()
        {
            if (_mortar == null)
            {
                _mortar = FindFirstObjectByType<MortarController>();
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(ResetPlayground);
            }

            // Record initial objects if spawn list is empty
            if (_spawnList.Count == 0)
            {
                CacheSceneObjects();
            }

            InitializeAmmo();
        }

        private void Update()
        {
            // Keyboard shortcut for quick iteration in Editor / Desktop (New Input System)
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetPlayground();
            }
        }

        private void CacheSceneObjects()
        {
            var physicsObjects = FindObjectsByType<PhysicsObject>(FindObjectsSortMode.None);
            foreach (var po in physicsObjects)
            {
                _activeObjects.Add(po.gameObject);
            }
        }

        /// <summary>
        /// Resets the playground: clears current active objects and projectiles,
        /// reinstantiates fresh target objects, and replenishes ammunition.
        /// </summary>
        public void ResetPlayground()
        {
            // Clean up any remaining physics objects and projectiles
            foreach (var obj in _activeObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _activeObjects.Clear();

            // Also clean up any loose projectile balls
            foreach (var proj in FindObjectsByType<ProjectileController>(FindObjectsSortMode.None))
            {
                if (proj != null) Destroy(proj.gameObject);
            }

            // Respawn all objects from config
            foreach (var info in _spawnList)
            {
                if (info.Prefab == null) continue;
                GameObject spawned = Instantiate(info.Prefab, info.Position, info.Rotation);
                _activeObjects.Add(spawned);
            }

            InitializeAmmo();
            Debug.Log("[PlaygroundController] Playground reset complete! Ammunition replenished.");
        }

        private void InitializeAmmo()
        {
            if (_mortar != null)
            {
                _mortar.ResetCooldown();
                _mortar.SetBallCount(_playgroundAmmo);
            }
        }

        /// <summary>
        /// Configure spawn targets programmatically (used by scene builders).
        /// </summary>
        public void ConfigureSpawns(List<ObjectSpawnInfo> spawns)
        {
            _spawnList = new List<ObjectSpawnInfo>(spawns);
        }
    }
}
