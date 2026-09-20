using System;
using UnityEngine;
using RoyalSmash.Save;
using RoyalSmash.Gameplay;

namespace RoyalSmash.Haptics
{
    /// <summary>
    /// Central manager for tactile haptic feedback.
    /// Manages vibration intensity scaling, save data synchronization, and device invocation.
    /// Follows Master Build Prompt Section 23 & Section 35 (Phase 18).
    /// </summary>
    public class HapticManager : MonoBehaviour
    {
        private static HapticManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static HapticManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<HapticManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("HapticManager");
                        _instance = go.AddComponent<HapticManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public bool IsVibrationEnabled { get; private set; } = true;
        public HapticType LastTriggeredHaptic { get; private set; } = HapticType.Light;
        public int TotalHapticsTriggered { get; private set; } = 0;

        public event Action<HapticType> OnHapticTriggered;

        private void Awake()
        {
            _isQuitting = false;
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent == null && Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
                SyncWithSaveData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SyncWithSaveData();
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveLoaded += HandleSaveLoaded;
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveLoaded -= HandleSaveLoaded;
            }
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void HandleSaveLoaded(PlayerSaveData data)
        {
            SyncWithSaveData();
        }

        public void SyncWithSaveData()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SetVibrationEnabled(SaveManager.Instance.Data.VibrationEnabled, false);
            }
            else
            {
                SetVibrationEnabled(true, false);
            }
        }

        public void SetVibrationEnabled(bool enabled, bool updateSave = true)
        {
            IsVibrationEnabled = enabled;

            if (updateSave && SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.VibrationEnabled = enabled;
                SaveManager.Instance.Save();
            }

            Debug.Log($"[HapticManager] Vibration set to {(enabled ? "ON" : "OFF")}.");
        }

        public void PlayHaptic(HapticType type)
        {
            if (!IsVibrationEnabled)
            {
                return;
            }

            LastTriggeredHaptic = type;
            TotalHapticsTriggered++;
            OnHapticTriggered?.Invoke(type);

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            // On mobile devices, invoke system vibration
            try
            {
                Handheld.Vibrate();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[HapticManager] Handheld.Vibrate error: {ex.Message}");
            }
#else
            Debug.Log($"[HapticManager] (Editor/Simulated) Played Haptic: {type}");
#endif
        }

        // =========================================================================
        // Section 23 Specific Haptic Triggers
        // =========================================================================

        public void TriggerFireHaptic()
        {
            PlayHaptic(HapticType.Medium);
        }

        public void TriggerImpactHaptic(ObjectType type, float impulse)
        {
            // Intensity scales with object weight
            bool isHeavy = type == ObjectType.Stone || type == ObjectType.Column || impulse > 3.0f;
            PlayHaptic(isHeavy ? HapticType.Heavy : HapticType.Light);
        }

        public void TriggerBottleBreakHaptic()
        {
            PlayHaptic(HapticType.MediumStrong);
        }

        public void TriggerSkillActivationHaptic()
        {
            PlayHaptic(HapticType.Medium);
        }

        public void TriggerWinHaptic()
        {
            PlayHaptic(HapticType.Heavy);
        }

        public void TriggerLoseHaptic()
        {
            PlayHaptic(HapticType.Light);
        }
    }
}
