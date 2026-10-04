using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Lives;
using RoyalSmash.Save;
using RoyalSmash.Levels;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Confirmation dialog displayed when player attempts to exit or restart an active level.
    /// Warns the player that quitting an active level deducts 1 life (per Section 1.3).
    /// </summary>
    public class ExitConfirmDialog : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private Button _confirmExitButton;
        [SerializeField] private Button _cancelButton;

        public event Action OnExitConfirmed;
        public event Action OnExitCancelled;

        private void Awake()
        {
            if (_confirmExitButton != null) _confirmExitButton.onClick.AddListener(HandleConfirmExit);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(HandleCancel);
        }

        public void BindReferences(GameObject root, Button confirmBtn, Button cancelBtn)
        {
            _contentRoot = root;
            _confirmExitButton = confirmBtn;
            _cancelButton = cancelBtn;

            if (_confirmExitButton != null)
            {
                _confirmExitButton.onClick.RemoveAllListeners();
                _confirmExitButton.onClick.AddListener(HandleConfirmExit);
            }
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(HandleCancel);
            }
        }

        public void Show()
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayPanelOpen();
            }

            GameObject target = _contentRoot != null ? _contentRoot : gameObject;
            UIPanelAnimator.AnimateIn(target);
        }

        public void Hide()
        {
            if (RoyalSmash.Audio.AudioManager.Instance != null)
            {
                RoyalSmash.Audio.AudioManager.Instance.PlayPanelClose();
            }

            GameObject target = _contentRoot != null ? _contentRoot : gameObject;
            UIPanelAnimator.AnimateOut(target);
        }

        private void HandleConfirmExit()
        {
            int levelIdx = 1;
            var levelMgr = LevelManager.Instance ?? FindFirstObjectByType<LevelManager>();
            if (levelMgr != null) levelIdx = levelMgr.CurrentLevelIndex;

            string transKey = $"Level_{levelIdx}_Exit";
            Debug.Log($"[ExitConfirmDialog] Player confirmed exit from active level. Deducting 1 life (key: {transKey}).");
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.DeductLife("LevelExit", transKey);
            }

            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.WasInActiveLevel = false;
                SaveManager.Instance.Save();
            }

            Hide();
            OnExitConfirmed?.Invoke();

            if (RoyalSmash.Core.SceneTransitionManager.Instance != null)
            {
                RoyalSmash.Core.SceneTransitionManager.Instance.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(RoyalSmash.Core.SceneTransitionManager.SCENE_MAIN_MENU);
            }
        }

        private void HandleCancel()
        {
            Hide();
            OnExitCancelled?.Invoke();
        }
    }
}
