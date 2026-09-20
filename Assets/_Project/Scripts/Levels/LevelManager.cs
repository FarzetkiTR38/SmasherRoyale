using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RoyalSmash.Economy;
using RoyalSmash.Gameplay;
using RoyalSmash.Save;
using RoyalSmash.Lives;
using RoyalSmash.Skills;

namespace RoyalSmash.Levels
{
    public enum LevelState
    {
        Playing,
        EvaluatingWin,
        EvaluatingLose,
        Won,
        Lost
    }

    /// <summary>
    /// Central manager for level execution, target tracking, physics settling verification,
    /// victory/defeat condition evaluation, and reward calculation.
    /// Follows the Master Build Prompt Section 10 rules.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private LevelRewardConfig _rewardConfig;
        [SerializeField] private LevelDatabase _levelDatabase;
        [SerializeField] private float _victoryDelay = 2.0f;
        [SerializeField] private float _physicsSettleVelocityThreshold = 0.2f;
        [SerializeField] private float _maxSettleWaitTime = 1.0f;
        [SerializeField] private int _currentLevelIndex = 1;

        [Header("Scene References")]
        [SerializeField] private MortarController _mortar;
        [SerializeField] private LevelLoader _levelLoader;

        // Runtime Tracking
        private readonly HashSet<PhysicsObject> _activeTargets = new HashSet<PhysicsObject>();
        private readonly HashSet<ProjectileController> _activeProjectiles = new HashSet<ProjectileController>();
        private int _totalTargets;
        private LevelState _currentState = LevelState.Playing;
        private Coroutine _evalRoutine;

        // Events
        public event Action<int, int> OnTargetCountChanged; // remaining, total
        public event Action<int, int, int, int> OnLevelWon; // level, baseReward, remainingBalls, totalGold
        public event Action<int, int> OnLevelLost;           // level, remainingTargets
        public event Action<LevelState> OnStateChanged;

        public LevelState CurrentState => _currentState;
        public int RemainingTargets => _activeTargets.Count;
        public int TotalTargets => _totalTargets;
        public int CurrentLevelIndex => _currentLevelIndex;
        public LevelRewardConfig RewardConfig => _rewardConfig;
        public LevelDatabase Database => _levelDatabase;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (_rewardConfig == null)
            {
                _rewardConfig = ScriptableObject.CreateInstance<LevelRewardConfig>();
            }

            if (_levelDatabase == null)
            {
#if UNITY_EDITOR
                _levelDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDatabase>(
                    "Assets/_Project/ScriptableObjects/Levels/LevelDatabase.asset");
#endif
            }
        }

        private void Start()
        {
            if (_mortar == null)
            {
                _mortar = FindFirstObjectByType<MortarController>();
            }

            if (_mortar != null)
            {
                _mortar.OnBallCountChanged += HandleBallCountChanged;
            }

            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayGameplayBGM();
            }

            // Sync current level index from SaveManager
            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                _currentLevelIndex = Mathf.Clamp(SaveManager.Instance.Data.CurrentLevel, 1, 30);
            }

            // Data-driven level loading via LevelLoader and LevelDatabase
            if (_levelLoader == null)
            {
                _levelLoader = FindFirstObjectByType<LevelLoader>();
            }

            if (_levelLoader != null && _levelDatabase != null)
            {
                var levelData = _levelDatabase.GetLevel(_currentLevelIndex);
                if (levelData != null)
                {
                    _levelLoader.LoadLevel(levelData);
                    if (_mortar != null)
                    {
                        _mortar.SetBallCount(levelData.BallCount);
                    }
                }
            }

            // Register spawned targets directly from LevelLoader, or fallback to scene scan
            if (_levelLoader != null && _levelLoader.SpawnedObjects.Count > 0)
            {
                ResetState();
                _activeTargets.Clear();
                foreach (var obj in _levelLoader.SpawnedObjects)
                {
                    RegisterTarget(obj);
                }
            }
            else
            {
                ScanAndRegisterSceneTargets();
            }

            // Mark active level session for ungraceful exit detection
            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.WasInActiveLevel = true;
                SaveManager.Instance.Data.ActiveLevelId = _currentLevelIndex;
                SaveManager.Instance.Save();
            }

            // Check if player has lives to play
            if (LivesManager.HasInstance && !LivesManager.Instance.HasLivesToPlay)
            {
                Debug.LogWarning("[LevelManager] Player has 0 lives! Gating level execution.");
                var outOfLives = FindFirstObjectByType<RoyalSmash.UI.OutOfLivesDialog>();
                if (outOfLives != null) outOfLives.Show();
            }

            // Sync skills with current level progression (Level 5 MegaBall, Level 10 InfinityBall)
            if (SkillManager.HasInstance)
            {
                SkillManager.Instance.UpdateUnlockStateForLevel(_currentLevelIndex);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_mortar != null)
            {
                _mortar.OnBallCountChanged -= HandleBallCountChanged;
            }
        }

        /// <summary>
        /// Registers a target PhysicsObject that must be cleared to win the level.
        /// </summary>
        public void RegisterTarget(PhysicsObject target)
        {
            if (target == null || _activeTargets.Contains(target)) return;

            _activeTargets.Add(target);
            _totalTargets = Mathf.Max(_totalTargets, _activeTargets.Count);

            target.OnStateChanged += HandleTargetStateChanged;
            OnTargetCountChanged?.Invoke(_activeTargets.Count, _totalTargets);
        }

        /// <summary>
        /// Unregisters a target PhysicsObject when destroyed or cleared.
        /// </summary>
        public void UnregisterTarget(PhysicsObject target)
        {
            if (target == null || !_activeTargets.Contains(target)) return;

            target.OnStateChanged -= HandleTargetStateChanged;
            _activeTargets.Remove(target);

            OnTargetCountChanged?.Invoke(_activeTargets.Count, _totalTargets);

            CheckConditions();
        }

        /// <summary>
        /// Registers a projectile currently in flight.
        /// </summary>
        public void RegisterProjectile(ProjectileController projectile)
        {
            if (projectile != null)
            {
                _activeProjectiles.Add(projectile);
            }
        }

        /// <summary>
        /// Unregisters a projectile when it impacts or auto-destroys.
        /// </summary>
        public void UnregisterProjectile(ProjectileController projectile)
        {
            if (projectile != null)
            {
                _activeProjectiles.Remove(projectile);
                CheckConditions();
            }
        }

        private void HandleTargetStateChanged(PhysicsObject obj, ObjectState state)
        {
            // Object is cleared if it enters destroy condition, destroying, or destroyed
            if (state == ObjectState.DestroyCondition || state == ObjectState.Destroying || state == ObjectState.Destroyed)
            {
                UnregisterTarget(obj);
            }
        }

        private void HandleBallCountChanged(int remainingBalls)
        {
            CheckConditions();
        }

        /// <summary>
        /// Resets the evaluation state back to Playing and halts any pending win/lose coroutines.
        /// </summary>
        public void ResetState()
        {
            if (_evalRoutine != null)
            {
                StopCoroutine(_evalRoutine);
                _evalRoutine = null;
            }
            _activeProjectiles.Clear();
            _currentState = LevelState.Playing;
            OnStateChanged?.Invoke(_currentState);
        }

        /// <summary>
        /// Explicitly cleans active projectile registry.
        /// </summary>
        public void ClearActiveProjectiles()
        {
            _activeProjectiles.Clear();
        }

        /// <summary>
        /// Scans active scene for all PhysicsObject components and registers them as required targets.
        /// </summary>
        public void ScanAndRegisterSceneTargets()
        {
            ResetState();
            _activeTargets.Clear();
            var objects = FindObjectsByType<PhysicsObject>(FindObjectsSortMode.None);
            foreach (var obj in objects)
            {
                if (obj.CurrentState != ObjectState.Destroyed && obj.CurrentState != ObjectState.Destroying && obj.CurrentState != ObjectState.DestroyCondition)
                {
                    _activeTargets.Add(obj);
                    obj.OnStateChanged += HandleTargetStateChanged;
                }
            }
            _totalTargets = _activeTargets.Count;
            OnTargetCountChanged?.Invoke(_activeTargets.Count, _totalTargets);
        }

        /// <summary>
        /// Evaluates whether win or lose conditions are met.
        /// </summary>
        public void CheckConditions()
        {
            if (_currentState == LevelState.Won || _currentState == LevelState.Lost) return;

            // Remove any null or destroyed projectile references
            _activeProjectiles.RemoveWhere(p => p == null);

            // 1. WIN EVALUATION: All required targets cleared
            if (_activeTargets.Count == 0 && _totalTargets > 0)
            {
                if (_currentState != LevelState.EvaluatingWin)
                {
                    if (_evalRoutine != null) StopCoroutine(_evalRoutine);
                    _evalRoutine = StartCoroutine(VictoryEvaluationRoutine());
                }
                return;
            }

            // 2. LOSE EVALUATION: 0 balls remaining AND all in-flight balls finished AND targets remain
            int remainingBalls = _mortar != null ? _mortar.RemainingBalls : 0;
            if (remainingBalls <= 0 && _activeProjectiles.Count == 0 && _activeTargets.Count > 0)
            {
                if (_currentState != LevelState.EvaluatingLose)
                {
                    if (_evalRoutine != null) StopCoroutine(_evalRoutine);
                    _evalRoutine = StartCoroutine(DefeatEvaluationRoutine());
                }
            }
        }

        private IEnumerator VictoryEvaluationRoutine()
        {
            _currentState = LevelState.EvaluatingWin;
            OnStateChanged?.Invoke(_currentState);

            // Physics Settling Check (brief settling window so pieces don't snap/freeze prematurely)
            float settleElapsed = 0f;
            while (settleElapsed < _maxSettleWaitTime)
            {
                settleElapsed += Time.deltaTime;
                bool allSettled = true;

                // Check if any registered rigidbodies are still moving rapidly
                foreach (var target in _activeTargets)
                {
                    if (target != null)
                    {
                        var rb = target.GetComponent<Rigidbody>();
                        if (rb != null && rb.linearVelocity.magnitude > _physicsSettleVelocityThreshold)
                        {
                            allSettled = false;
                            break;
                        }
                    }
                }

                if (allSettled && settleElapsed >= 0.3f) break;
                yield return null;
            }

            // Victory delay (~2 sec configurable) before showing Win Panel
            yield return new WaitForSeconds(_victoryDelay);

            _currentState = LevelState.Won;
            OnStateChanged?.Invoke(_currentState);

            // Clear active level session flag (graceful completion, 0 life loss)
            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.WasInActiveLevel = false;
                SaveManager.Instance.Save();
            }

            int remainingBalls = _mortar != null ? _mortar.RemainingBalls : 0;
            int baseReward = _rewardConfig != null ? _rewardConfig.BaseLevelReward : 50;
            int totalGold = _rewardConfig != null ? _rewardConfig.CalculateReward(remainingBalls) : baseReward + (remainingBalls * 2);

            // Clean up any lingering active skill states
            if (SkillManager.HasInstance)
            {
                SkillManager.Instance.ResetActiveSkills();
            }

            Debug.Log($"[LevelManager] VICTORY! Level {_currentLevelIndex} Cleared! Base: {baseReward}, Remaining Balls: {remainingBalls}, Total Gold: {totalGold}");
            OnLevelWon?.Invoke(_currentLevelIndex, baseReward, remainingBalls, totalGold);

            if (RoyalSmash.Ads.AdManager.Instance != null)
            {
                RoyalSmash.Ads.AdManager.Instance.CheckAndShowProgressionInterstitial(_currentLevelIndex);
            }
        }

        private IEnumerator DefeatEvaluationRoutine()
        {
            _currentState = LevelState.EvaluatingLose;
            OnStateChanged?.Invoke(_currentState);

            // Wait a brief moment for any lingering physical impacts to settle
            yield return new WaitForSeconds(1.0f);

            // Double check targets still remain and no ball cleared the level during settle
            if (_activeTargets.Count > 0)
            {
                _currentState = LevelState.Lost;
                OnStateChanged?.Invoke(_currentState);

                // Clear active level flag and deduct exactly 1 life for defeat
                if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
                {
                    SaveManager.Instance.Data.WasInActiveLevel = false;
                    SaveManager.Instance.Save();
                }

                if (LivesManager.HasInstance)
                {
                    LivesManager.Instance.DeductLife("LevelLost", $"Level_{_currentLevelIndex}_Loss");
                }

                // Clean up any lingering active skill states
                if (SkillManager.HasInstance)
                {
                    SkillManager.Instance.ResetActiveSkills();
                }

                Debug.Log($"[LevelManager] DEFEAT! Balls depleted with {_activeTargets.Count} targets remaining.");
                OnLevelLost?.Invoke(_currentLevelIndex, _activeTargets.Count);
            }
            else
            {
                // Last ball actually cleared the puzzle!
                StartCoroutine(VictoryEvaluationRoutine());
            }
        }

        /// <summary>
        /// Restarts the active scene/level (called by Retry button).
        /// If called while actively playing, deducts 1 life as exit penalty.
        /// </summary>
        public void RestartLevel()
        {
            if (_currentState == LevelState.Playing)
            {
                if (LivesManager.HasInstance)
                {
                    LivesManager.Instance.DeductLife("LevelExit", $"Level_{_currentLevelIndex}_Exit");
                }
            }

            if (SkillManager.HasInstance)
            {
                SkillManager.Instance.ResetActiveSkills();
            }

            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.WasInActiveLevel = false;
                SaveManager.Instance.Save();
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Advances to next level or returns to MainMenu if at Level 30.
        /// </summary>
        public void NextLevel()
        {
            Debug.Log($"[LevelManager] NextLevel requested from Level {_currentLevelIndex}.");
            if (_currentLevelIndex >= 30)
            {
                Debug.Log("[LevelManager] Level 30 finale reached! Returning to Main Menu.");
                if (Core.SceneTransitionManager.Instance != null)
                {
                    Core.SceneTransitionManager.Instance.LoadMainMenu();
                }
                else
                {
                    SceneManager.LoadScene("MainMenu");
                }
                return;
            }

            _currentLevelIndex++;
            if (SaveManager.HasInstance && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.CurrentLevel = _currentLevelIndex;
                SaveManager.Instance.Save();
            }

            RestartLevel();
        }

        /// <summary>
        /// Sets current level index.
        /// </summary>
        public void SetLevelIndex(int level)
        {
            _currentLevelIndex = level;
        }
    }
}
