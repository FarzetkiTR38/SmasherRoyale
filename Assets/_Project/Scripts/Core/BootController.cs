using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using RoyalSmash.Save;
using RoyalSmash.Profile;
using RoyalSmash.Economy;
using RoyalSmash.Lives;
using RoyalSmash.Skills;
using RoyalSmash.Audio;
using RoyalSmash.Haptics;
using RoyalSmash.VFX;
using RoyalSmash.Cloud;
using RoyalSmash.Ads;

namespace RoyalSmash.Core
{
    /// <summary>
    /// Master controller for BootScene.
    /// Manages the visual loading screen, cycles gameplay tips, initializes all persistent singletons,
    /// synchronizes cloud/save data, and smoothly transitions to the next scene.
    /// Template elements are exposed in Inspector so the user can easily swap background and logo sprites.
    /// </summary>
    public class BootController : MonoBehaviour
    {
        private static BootController _instance;
        public static BootController Instance => _instance;

        [Header("UI Template Slots (Easily Swappable by User)")]
        [Tooltip("Full-screen background artwork. Drag & drop your custom background sprite here.")]
        [SerializeField] private Image _backgroundImage;

        [Tooltip("Game logo at upper-center. Drag & drop your custom logo sprite here.")]
        [SerializeField] private Image _logoImage;

        [Tooltip("Glowing loading slider/progress bar at lower screen.")]
        [SerializeField] private Slider _loadingSlider;

        [Tooltip("Gameplay hint/tip above the progress bar.")]
        [SerializeField] private TextMeshProUGUI _hintText;

        [Tooltip("Status text below the progress bar (e.g., 'YUKLENIYOR...').")]
        [SerializeField] private TextMeshProUGUI _statusText;

        [Tooltip("Optional percentage text indicator.")]
        [SerializeField] private TextMeshProUGUI _percentText;

        [Header("Timing Settings")]
        [SerializeField] private float _minLoadTime = 1.6f;
        [SerializeField] private float _sliderSmoothSpeed = 3.5f;
        [SerializeField] private float _hintCycleInterval = 3.0f;

        [Header("Gameplay Hints / Tips")]
        [SerializeField] private string[] _hints = new string[]
        {
            "Her Rift bir zayıf nokta saklar.",
            "Mega Ball en ağır sütun ve taş blokları bile devirebilir!",
            "Zayıf noktalara vurarak devasa zincirleme tepkiler başlatın!",
            "Infinity Ball 5 saniye boyunca sınırsız ve beklemesiz atış kazandırır.",
            "Topları standın kenarından sektirerek arkadaki hedefleri vurun!",
            "Boya şişeleri kırıldığında etrafa kaygan boya saçar.",
            "Seviyeyi daha az topla bitirmek ekstra altın bonusu kazandırır!",
            "Havanı sürükleyerek nişan alın; dokunduğunuz noktaya ateş edersiniz."
        };

        private float _targetProgress = 0f;
        private float _displayedProgress = 0f;
        private bool _isInitializationDone = false;
        private Coroutine _hintRoutine;

        // Public getters for testing / inspection
        public Image BackgroundImage => _backgroundImage;
        public Image LogoImage => _logoImage;
        public Slider LoadingSlider => _loadingSlider;
        public TextMeshProUGUI HintText => _hintText;
        public TextMeshProUGUI StatusText => _statusText;
        public float DisplayedProgress => _displayedProgress;
        public bool IsInitializationDone => _isInitializationDone;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            EnsureUIReferences();
            StartCoroutine(InitializeBootPipeline());
            StartHintRotation();
        }

        private void Update()
        {
            // Smoothly interpolate slider fill towards target progress
            if (_displayedProgress < _targetProgress)
            {
                _displayedProgress = Mathf.MoveTowards(_displayedProgress, _targetProgress, Time.deltaTime * _sliderSmoothSpeed);
                UpdateSliderUI(_displayedProgress);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
            if (_hintRoutine != null) StopCoroutine(_hintRoutine);
        }

        public void SetTargetProgress(float progress, string statusMessage = null)
        {
            _targetProgress = Mathf.Clamp01(progress);
            if (!string.IsNullOrEmpty(statusMessage) && _statusText != null)
            {
                _statusText.text = statusMessage;
            }
        }

        private void UpdateSliderUI(float progress01)
        {
            if (_loadingSlider != null)
            {
                _loadingSlider.value = progress01;
            }

            if (_percentText != null)
            {
                _percentText.text = $"{Mathf.RoundToInt(progress01 * 100f)}%";
            }
        }

        private void EnsureUIReferences()
        {
            if (_loadingSlider == null) _loadingSlider = GetComponentInChildren<Slider>();
            if (_hintText == null)
            {
                var texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) _hintText = texts[0];
            }
        }

        private void StartHintRotation()
        {
            if (_hints != null && _hints.Length > 0 && _hintText != null)
            {
                _hintText.text = _hints[0];
                if (_hintRoutine != null) StopCoroutine(_hintRoutine);
                _hintRoutine = StartCoroutine(HintRotationCoroutine());
            }
        }

        private IEnumerator HintRotationCoroutine()
        {
            int index = 0;
            while (!_isInitializationDone)
            {
                yield return new WaitForSecondsRealtime(_hintCycleInterval);
                if (_hints != null && _hints.Length > 0 && _hintText != null)
                {
                    index = (index + 1) % _hints.Length;
                    _hintText.text = _hints[index];
                }
            }
        }

        /// <summary>
        /// Main asynchronous boot pipeline. Initializes all core systems step-by-step.
        /// </summary>
        private IEnumerator InitializeBootPipeline()
        {
            float startTime = Time.realtimeSinceStartup;

            // Step 1: Ensure all global managers exist as persistent singletons (0% -> 20%)
            SetTargetProgress(0.15f, "YÖNETİCİLER BAŞLATILIYOR...");
            EnsureGlobalManagers();
            yield return new WaitForSeconds(0.2f);

            // Step 2: Save Data & Player Profile (20% -> 40%)
            SetTargetProgress(0.35f, "OYUNCU VERİLERİ YÜKLENİYOR...");
            if (SaveManager.Instance != null && SaveManager.Instance.Data == null)
            {
                SaveManager.Instance.Load();
            }
            yield return new WaitForSeconds(0.2f);

            // Step 3: Economy, Lives & Skills (40% -> 60%)
            SetTargetProgress(0.55f, "CAN VE EKONOMİ EŞİTLENİYOR...");
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.ProcessRegeneration();
            }
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.InitializeGold();
            }
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.SyncWithSaveData();
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SyncWithSaveData();
            }
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.SyncWithSaveData();
            }
            yield return new WaitForSeconds(0.2f);

            // Step 4: Cloud Sync & Leaderboard (60% -> 80%)
            SetTargetProgress(0.75f, "BULUT VE LİDER TABLOSU BAĞLANTISI...");
            if (CloudSyncManager.Instance != null)
            {
                CloudSyncManager.Instance.TriggerSync();
            }
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.FetchLeaderboard();
            }
            yield return new WaitForSeconds(0.2f);

            // Step 5: Ads & IAP Systems (80% -> 90%)
            SetTargetProgress(0.90f, "HİZMETLER HAZIRLANIYOR...");
            yield return new WaitForSeconds(0.15f);

            // Wait until minimum display time has passed for smooth perception
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _minLoadTime)
            {
                yield return new WaitForSeconds(_minLoadTime - elapsed);
            }

            // Step 6: 100% Ready
            SetTargetProgress(1.0f, "YÜKLENDİ!");
            while (_displayedProgress < 0.99f)
            {
                yield return null;
            }
            _displayedProgress = 1.0f;
            UpdateSliderUI(1.0f);

            _isInitializationDone = true;
            yield return new WaitForSeconds(0.25f);

            // Step 7: Route to Destination Scene
            RouteToDestination();
        }

        /// <summary>
        /// Instantiates all core global managers if not already present in the scene hierarchy.
        /// </summary>
        public void EnsureGlobalManagers()
        {
            EnsureManager<SaveManager>("SaveManager");
            EnsureManager<ProfileManager>("ProfileManager");
            EnsureManager<GoldManager>("GoldManager");
            EnsureManager<LivesManager>("LivesManager");
            EnsureManager<SkillManager>("SkillManager");
            EnsureManager<AudioManager>("AudioManager");
            EnsureManager<HapticManager>("HapticManager");
            EnsureManager<VFXManager>("VFXManager");
            EnsureManager<CloudSyncManager>("CloudSyncManager");
            EnsureManager<LeaderboardManager>("LeaderboardManager");
            EnsureManager<AdManager>("AdManager");
            EnsureManager<IAPManager>("IAPManager");
            EnsureManager<SceneTransitionManager>("SceneTransitionManager");
        }

        private T EnsureManager<T>(string gameObjectName) where T : MonoBehaviour
        {
            var existing = FindFirstObjectByType<T>();
            if (existing == null)
            {
                var go = new GameObject(gameObjectName);
                existing = go.AddComponent<T>();
                DontDestroyOnLoad(go);
                Debug.Log($"[BootController] Created persistent singleton: {gameObjectName}");
            }
            return existing;
        }

        /// <summary>
        /// Routes the player to MainGame (if new/tutorial uncompleted) or MainMenu (if returning player).
        /// </summary>
        public void RouteToDestination()
        {
            string destinationScene = DetermineDestinationScene();
            Debug.Log($"[BootController] Boot sequence complete. Transitioning to '{destinationScene}'...");

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(destinationScene);
            }
            else
            {
                SceneManager.LoadScene(destinationScene);
            }
        }

        public string DetermineDestinationScene()
        {
            return SceneTransitionManager.SCENE_MAIN_MENU;
        }
    }
}
