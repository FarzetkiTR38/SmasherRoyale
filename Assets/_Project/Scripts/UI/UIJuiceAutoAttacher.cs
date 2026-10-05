using UnityEngine;
using UnityEngine.UI;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Autonomous runtime and editor assistant that ensures 100% of UI buttons in the game
    /// possess tactile micro-interactions (UIButtonJuice) and key call-to-action buttons
    /// (Play / Savaş button) possess the breathing pulse effect (UIButtonPulseEffect).
    /// Follows Phase 36 Visual Polish specifications.
    /// </summary>
    public static class UIJuiceAutoAttacher
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            ApplyJuiceToAllButtonsInScene();
        }

        /// <summary>
        /// Scans all active and inactive Buttons in loaded scenes, ensuring UIButtonJuice is attached.
        /// </summary>
        public static void ApplyJuiceToAllButtonsInScene()
        {
            var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int juiceAdded = 0;
            int pulseAdded = 0;

            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                // 1. Ensure tactile press juice
                var juice = btn.GetComponent<UIButtonJuice>();
                if (juice == null)
                {
                    btn.gameObject.AddComponent<UIButtonJuice>();
                    juiceAdded++;
                }

                // 2. Detect Main Menu Play / Battle call-to-action button (exclude navigation tabs)
                string lowerName = btn.gameObject.name.ToLowerInvariant();
                bool isPlayBtn = (lowerName.Contains("play") || lowerName.Contains("savas") || lowerName.Contains("oyna") || (lowerName.Contains("battle") && !lowerName.Contains("tab"))) && !lowerName.Contains("tab");

                if (isPlayBtn)
                {
                    var pulse = btn.GetComponent<UIButtonPulseEffect>();
                    if (pulse == null)
                    {
                        btn.gameObject.AddComponent<UIButtonPulseEffect>();
                        pulseAdded++;
                    }
                }
            }

            if (juiceAdded > 0 || pulseAdded > 0)
            {
                Debug.Log($"[UIJuiceAutoAttacher] Initialized micro-interactions: {juiceAdded} UIButtonJuice, {pulseAdded} UIButtonPulseEffect.");
            }
        }
    }
}
