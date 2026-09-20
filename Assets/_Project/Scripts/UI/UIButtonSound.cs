using UnityEngine;
using UnityEngine.UI;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Helper component attached to UI Buttons to automatically trigger the click SFX.
    /// Follows Master Build Prompt Section 21 & Section 35 (Phase 16).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleButtonClicked);
            }
        }

        private void HandleButtonClicked()
        {
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
