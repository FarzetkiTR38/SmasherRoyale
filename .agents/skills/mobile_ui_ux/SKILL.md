---
name: Mobile UI & UX Patterns
description: >
  Mobile-first UI/UX patterns for casual puzzle games in Unity.
  Covers TextMeshPro usage, responsive layouts, panel management, HUD design,
  touch-friendly button sizing, safe area handling, and UI animation patterns.
---

# Mobile UI & UX Patterns Skill

## Applies To
All UI scripts, prefabs, canvases, and panel management in this project.

---

## 1. Canvas Setup

### Canvas Configuration
```
Main Canvas:
  Render Mode: Screen Space - Overlay
  UI Scale Mode: Scale With Screen Size
  Reference Resolution: 1080 x 1920 (Portrait)
  Screen Match Mode: Match Width or Height
  Match: 0.5 (balanced between width and height)
```

### Canvas Hierarchy (Separate for Performance)
```
├── Canvas_Background    (Static backgrounds, rarely changes)
│   └── Order: 0
├── Canvas_Gameplay      (HUD during gameplay)
│   └── Order: 10
├── Canvas_Panels        (Win/Lose/Shop/Profile panels)
│   └── Order: 20
├── Canvas_Popup         (Confirmation dialogs, alerts)
│   └── Order: 30
├── Canvas_Loading       (Loading screen, splash)
│   └── Order: 40
```

Why separate canvases? When one UI element changes, Unity rebuilds the ENTIRE canvas mesh. Separating static from dynamic prevents unnecessary rebuilds.

---

## 2. TextMeshPro — MANDATORY

```csharp
// ALWAYS use TextMeshPro — NEVER use legacy Text
using TMPro;

// In code:
[SerializeField] private TextMeshProUGUI _goldText;
[SerializeField] private TextMeshProUGUI _ballCountText;
[SerializeField] private TMP_InputField _usernameInput;

// Update text efficiently — avoid per-frame string allocations
private int _lastDisplayedGold = -1;
public void UpdateGoldDisplay(int gold)
{
    if (gold == _lastDisplayedGold) return; // Skip if unchanged
    _lastDisplayedGold = gold;
    _goldText.text = gold.ToString("N0"); // "1,250" format
}
```

### Font Setup
- Use a clean, readable font (Inter, Poppins, Nunito, or similar).
- Import as SDF font asset for crisp rendering at all sizes.
- Set `Material Preset: Distance Field` for best quality.

---

## 3. Safe Area Handling

```csharp
/// <summary>
/// Adjusts a RectTransform to respect the device safe area (notch, rounded corners).
/// Attach to the root container of each canvas.
/// </summary>
public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        if (safeArea == _lastSafeArea) return;
        _lastSafeArea = safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
    }
}
```

---

## 4. Touch-Friendly UI Guidelines

### Button Sizing
- **Minimum touch target**: 44×44 dp (approximately 88×88 pixels at 2× density).
- **Recommended**: 48×48 dp or larger for primary actions.
- **Spacing**: At least 8dp between interactive elements.

### Button Click Prevention (Anti-Double-Click)
```csharp
public class SafeButton : MonoBehaviour
{
    [SerializeField] private float _cooldownDuration = 0.3f;
    private Button _button;
    private float _lastClickTime;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (Time.unscaledTime - _lastClickTime < _cooldownDuration)
        {
            return; // Ignore rapid clicks
        }
        _lastClickTime = Time.unscaledTime;
        // Proceed with actual action
    }
}
```

---

## 5. Panel Management Pattern

```csharp
namespace RoyalSmash.UI
{
    /// <summary>
    /// Base class for all UI panels (Win, Lose, Shop, Profile, Settings, etc.)
    /// Handles show/hide with animation support.
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        public bool IsVisible { get; private set; }
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            StartCoroutine(FadeIn());
        }
        
        public virtual void Hide()
        {
            IsVisible = false;
            StartCoroutine(FadeOut());
        }
        
        private IEnumerator FadeIn()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            
            float elapsed = 0;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = elapsed / _fadeDuration;
                yield return null;
            }
            
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
        }
        
        private IEnumerator FadeOut()
        {
            _canvasGroup.interactable = false;
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0;
            
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, elapsed / _fadeDuration);
                yield return null;
            }
            
            _canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
    }
}
```

### Panel Manager
```csharp
/// <summary>
/// Manages showing/hiding UI panels. Ensures only one popup is visible at a time.
/// </summary>
public class UIPanelManager : MonoBehaviour
{
    [SerializeField] private WinPanel _winPanel;
    [SerializeField] private LosePanel _losePanel;
    [SerializeField] private SettingsPanel _settingsPanel;
    [SerializeField] private ShopPanel _shopPanel;
    [SerializeField] private ProfilePanel _profilePanel;
    [SerializeField] private ConfirmationPopup _confirmPopup;
    
    private UIPanel _currentPanel;
    
    public void ShowPanel(UIPanel panel)
    {
        if (_currentPanel != null && _currentPanel.IsVisible)
            _currentPanel.Hide();
        
        _currentPanel = panel;
        panel.Show();
    }
}
```

---

## 6. HUD Pattern (Gameplay)

```csharp
/// <summary>
/// Gameplay HUD — minimal, non-obstructive.
/// Shows: Remaining Balls, Settings button, Skill buttons.
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI _ballCountText;
    [SerializeField] private Button _settingsButton;
    
    [Header("Skill Buttons")]
    [SerializeField] private SkillButton _megaBallButton;
    [SerializeField] private SkillButton _infinityBallButton;
    
    /// <summary>
    /// Update ball count display. Called after each fire.
    /// </summary>
    public void UpdateBallCount(int remaining)
    {
        _ballCountText.text = remaining.ToString();
        
        // Optional: animate when low (e.g., < 5 balls)
        if (remaining <= 5)
            _ballCountText.color = Color.red;
    }
}
```

### Skill Button States
```csharp
public class SkillButton : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private GameObject _lockOverlay;
    [SerializeField] private GameObject _priceTag;
    
    public void SetLocked()
    {
        _lockOverlay.SetActive(true);
        _countText.gameObject.SetActive(false);
        _timerText.gameObject.SetActive(false);
        _priceTag.SetActive(false);
    }
    
    public void SetAvailable(int remainingUses)
    {
        _lockOverlay.SetActive(false);
        _countText.gameObject.SetActive(true);
        _countText.text = remainingUses.ToString();
        _timerText.gameObject.SetActive(false);
        _priceTag.SetActive(false);
    }
    
    public void SetActive(float remainingTime)
    {
        _timerText.gameObject.SetActive(true);
        _timerText.text = remainingTime.ToString("F1");
        _countText.gameObject.SetActive(false);
    }
    
    public void SetDepleted(string price)
    {
        _lockOverlay.SetActive(false);
        _countText.gameObject.SetActive(false);
        _priceTag.SetActive(true);
        _priceTag.GetComponentInChildren<TextMeshProUGUI>().text = price;
    }
}
```

---

## 7. UI Animation Patterns

### DOTween-Style Tweening (or use Unity Coroutines)
```csharp
/// <summary>
/// Simple scale punch animation for button press feedback.
/// </summary>
public static IEnumerator PunchScale(Transform target, float intensity = 0.1f, float duration = 0.2f)
{
    Vector3 originalScale = target.localScale;
    Vector3 punchScale = originalScale * (1 + intensity);
    
    float half = duration / 2f;
    float elapsed = 0;
    
    // Scale up
    while (elapsed < half)
    {
        elapsed += Time.unscaledDeltaTime;
        target.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / half);
        yield return null;
    }
    
    elapsed = 0;
    // Scale down
    while (elapsed < half)
    {
        elapsed += Time.unscaledDeltaTime;
        target.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / half);
        yield return null;
    }
    
    target.localScale = originalScale;
}

/// <summary>
/// Gold counter rolling animation.
/// </summary>
public static IEnumerator AnimateCounter(TextMeshProUGUI text, int from, int to, float duration = 0.5f)
{
    float elapsed = 0;
    while (elapsed < duration)
    {
        elapsed += Time.unscaledDeltaTime;
        int current = (int)Mathf.Lerp(from, to, elapsed / duration);
        text.text = current.ToString("N0");
        yield return null;
    }
    text.text = to.ToString("N0");
}
```

---

## 8. Main Menu Layout Pattern

```
Portrait Layout (1080 x 1920):

[0, 1920] ┌────────────────────────────┐
           │ SafeArea Top Padding       │
[~1800]    │ [Avatar]        [⚙ Settings]│
           │                            │
[~1700]    │    🪙 1,250    ❤️ 5/5      │
           │                            │
           │                            │
[~1200]    │         LEVEL 12           │
           │                            │
[~1000]    │        [ PLAY ]            │
           │                            │
[~700]     │ [Offer 1]    [Offer 2]     │
           │                            │
           │                            │
[~200]     │                            │
           │ [Shop] [Home] [Leaderboard]│
[0]        │ SafeArea Bottom Padding    │
           └────────────────────────────┘
```

---

## 9. Responsive Layout Rules

- Use **anchors** and **pivot points** for all UI elements.
- Use **Layout Groups** (Vertical/Horizontal) for dynamic lists.
- Use **Content Size Fitter** for text that changes length.
- Use **Aspect Ratio Fitter** for images that must maintain ratio.
- Test on: 16:9, 18:9, 19.5:9, 20:9 aspect ratios.
- Test on: iPhone SE (small), iPhone 15 Pro Max (large), various Android sizes.

---

## 10. Loading Screen Pattern

```csharp
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _tipText;
    [SerializeField] private string[] _tips;
    
    public void Show()
    {
        gameObject.SetActive(true);
        _progressBar.value = 0;
        _tipText.text = _tips[UnityEngine.Random.Range(0, _tips.Length)];
    }
    
    public void UpdateProgress(float progress)
    {
        _progressBar.value = progress;
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
```
