using TMPro;
using UnityEngine;
using RoyalSmash.Levels;

namespace RoyalSmash.UI
{
    /// <summary>Displays the loaded level and keeps the themed HUD within device safe-area insets.</summary>
    [DisallowMultipleComponent]
    public sealed class SkyCitadelStageLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private RectTransform[] _topWidgets;
        [SerializeField] private RectTransform[] _bottomWidgets;

        private Canvas _canvas;
        private Vector2[] _topPositions;
        private Vector2[] _bottomPositions;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private float _lastScale;
        private int _lastLevel = -1;

        public void Configure(TextMeshProUGUI levelText, RectTransform[] topWidgets, RectTransform[] bottomWidgets)
        {
            _levelText = levelText;
            _topWidgets = topWidgets;
            _bottomWidgets = bottomWidgets;
        }

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _topPositions = CapturePositions(_topWidgets);
            _bottomPositions = CapturePositions(_bottomWidgets);
        }

        private void LateUpdate()
        {
            // LevelManager finishes loading in Start; the integer comparison avoids repeated text allocations.
            var manager = LevelManager.Instance;
            if (manager != null && manager.CurrentLevelIndex != _lastLevel && _levelText != null)
            {
                _lastLevel = manager.CurrentLevelIndex;
                _levelText.SetText("{0}", _lastLevel);
            }

            if (_canvas == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(0.01f, _canvas.scaleFactor);
            var size = new Vector2Int(Screen.width, Screen.height);
            if (safe == _lastSafeArea && size == _lastScreenSize && Mathf.Approximately(scale, _lastScale)) return;
            _lastSafeArea = safe;
            _lastScreenSize = size;
            _lastScale = scale;
            ApplyInsets(_topWidgets, _topPositions, -(Screen.height - safe.yMax) / scale, safe, scale);
            ApplyInsets(_bottomWidgets, _bottomPositions, safe.yMin / scale, safe, scale);
        }

        private static Vector2[] CapturePositions(RectTransform[] widgets)
        {
            if (widgets == null) return new Vector2[0];
            var positions = new Vector2[widgets.Length];
            for (int i = 0; i < widgets.Length; i++)
                if (widgets[i] != null) positions[i] = widgets[i].anchoredPosition;
            return positions;
        }

        private static void ApplyInsets(RectTransform[] widgets, Vector2[] positions, float verticalInset, Rect safe, float scale)
        {
            if (widgets == null || positions == null) return;
            for (int i = 0; i < widgets.Length && i < positions.Length; i++)
            {
                var widget = widgets[i];
                if (widget == null) continue;
                float horizontalInset = widget.anchorMin.x < 0.25f ? safe.xMin / scale :
                    widget.anchorMax.x > 0.75f ? -(Screen.width - safe.xMax) / scale : 0f;
                widget.anchoredPosition = positions[i] + new Vector2(horizontalInset, verticalInset);
            }
        }
    }
}
