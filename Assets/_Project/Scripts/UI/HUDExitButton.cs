using UnityEngine;
using UnityEngine.UI;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Attached to HUD Exit / Quit button.
    /// Opens the ExitConfirmDialog to confirm level abandonment.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class HUDExitButton : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(HandleExitClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleExitClicked);
            }
        }

        private void HandleExitClicked()
        {
            var dialog = FindFirstObjectByType<ExitConfirmDialog>();
            if (dialog != null)
            {
                dialog.Show();
            }
            else
            {
                Debug.LogWarning("[HUDExitButton] ExitConfirmDialog not found in scene!");
            }
        }
    }
}
