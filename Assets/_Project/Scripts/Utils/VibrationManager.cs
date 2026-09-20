using System;
using UnityEngine;
using RoyalSmash.Save;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Central manager for haptic feedback and vibrations.
    /// Synchronizes with PlayerSaveData.VibrationEnabled setting (Section 20).
    /// </summary>
    public class VibrationManager : MonoBehaviour
    {
        private static VibrationManager _instance;
        private static bool _isQuitting = false;
        public static bool HasInstance => _instance != null;

        public static VibrationManager Instance
        {
            get
            {
                if (_instance == null && !_isQuitting)
                {
                    _instance = FindFirstObjectByType<VibrationManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        var go = new GameObject("VibrationManager");
                        _instance = go.AddComponent<VibrationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public bool IsVibrationEnabled { get; private set; } = true;
        public event Action<bool> OnVibrationStateChanged;

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
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Start()
        {
            SyncWithSaveData();
        }

        public void SyncWithSaveData()
        {
            var save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            IsVibrationEnabled = save == null || save.VibrationEnabled;
        }

        public void SetVibrationEnabled(bool enabled, bool updateSave = true)
        {
            IsVibrationEnabled = enabled;
            if (updateSave && SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.VibrationEnabled = enabled;
                SaveManager.Instance.Save();
            }
            OnVibrationStateChanged?.Invoke(enabled);
            Debug.Log($"[VibrationManager] Vibration set to {(enabled ? "ON" : "OFF")}.");
        }

        public void TriggerHaptic()
        {
            if (!IsVibrationEnabled) return;

#if UNITY_ANDROID || UNITY_IOS
            try
            {
                Handheld.Vibrate();
            }
            catch (Exception ex)
            {
                Debug.Log($"[VibrationManager] Haptic trigger: {ex.Message}");
            }
#else
            Debug.Log("[VibrationManager] Haptic triggered (Editor simulation).");
#endif
        }
    }
}
