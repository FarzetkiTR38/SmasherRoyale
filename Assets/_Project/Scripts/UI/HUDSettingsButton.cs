using UnityEngine;
using UnityEngine.UI;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Attached to HUD Settings button (top-right gear icon).
    /// Opens the InGameSettingsDialog modal.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class HUDSettingsButton : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            var dialog = InGameSettingsDialog.Instance ?? FindFirstObjectByType<InGameSettingsDialog>();
            if (dialog != null)
            {
                dialog.Show();
            }
            else
            {
                Debug.LogWarning("[HUDSettingsButton] InGameSettingsDialog not found!");
            }
        }
    }
}
