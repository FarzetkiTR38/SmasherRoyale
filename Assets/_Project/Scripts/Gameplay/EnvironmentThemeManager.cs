using System;
using System.Collections;
using UnityEngine;
using RoyalSmash.Levels;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Central manager for dynamic 3D world environment themes, lighting, sky gradient, and atmospheric fog.
    /// Synchronizes scene atmosphere with player progression milestones across all 30 levels.
    /// Follows Master Build Prompt Section 25 & Full Documentation Sections 297-298.
    /// </summary>
    public class EnvironmentThemeManager : MonoBehaviour
    {
        private static EnvironmentThemeManager _instance;
        public static EnvironmentThemeManager Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private EnvironmentConfig _config;

        [Header("Scene References")]
        [SerializeField] private Light _directionalLight;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform _backdropRoot;

        private EnvironmentTheme _currentTheme;
        public EnvironmentTheme CurrentTheme => _currentTheme;

        public EnvironmentConfig Config
        {
            get => _config;
            set => _config = value;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            FindSceneReferences();
        }

        private void Start()
        {
            FindSceneReferences();

            // Subscribe to LevelManager if available
            if (LevelManager.Instance != null)
            {
                ApplyThemeForLevel(LevelManager.Instance.CurrentLevelIndex);
            }
            else
            {
                ApplyThemeForLevel(1);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void FindSceneReferences()
        {
            if (_directionalLight == null)
            {
                var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                foreach (var l in lights)
                {
                    if (l.type == LightType.Directional)
                    {
                        _directionalLight = l;
                        break;
                    }
                }
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_backdropRoot == null)
            {
                var bg = GameObject.Find("KingdomBackdrop");
                if (bg != null) _backdropRoot = bg.transform;
            }
        }

        /// <summary>
        /// Applies lighting, fog, and sky styling for the specified level.
        /// </summary>
        public void ApplyThemeForLevel(int level)
        {
            if (_config == null)
            {
#if UNITY_EDITOR
                _config = UnityEditor.AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(
                    "Assets/_Project/ScriptableObjects/Config/EnvironmentConfig.asset");
#endif
            }

            EnvironmentTheme targetTheme = _config != null ? _config.GetThemeForLevel(level) : EnvironmentConfig.GetDefaultTheme();
            ApplyTheme(targetTheme);
        }

        /// <summary>
        /// Applies the given EnvironmentTheme immediately to the active scene.
        /// </summary>
        public void ApplyTheme(EnvironmentTheme theme)
        {
            if (theme == null) return;
            _currentTheme = theme;

            FindSceneReferences();

            // 1. Directional Sun Light
            if (_directionalLight != null)
            {
                _directionalLight.color = theme.SunLightColor;
                _directionalLight.intensity = theme.SunLightIntensity;
                _directionalLight.transform.rotation = Quaternion.Euler(theme.SunEulerAngles);
            }

            // 2. Camera Background
            if (_mainCamera != null)
            {
                _mainCamera.clearFlags = CameraClearFlags.SolidColor;
                _mainCamera.backgroundColor = theme.SkyHorizonColor;
            }

            // 3. Atmospheric Fog
            RenderSettings.fog = theme.EnableFog;
            if (theme.EnableFog)
            {
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogColor = theme.FogColor;
                RenderSettings.fogStartDistance = theme.FogStartDistance;
                RenderSettings.fogEndDistance = theme.FogEndDistance;
            }

            Debug.Log($"[EnvironmentThemeManager] Applied theme: '{theme.ThemeName}' (Fog: {theme.EnableFog}, Sun: {theme.SunLightIntensity:F2}x)");
        }
    }
}
