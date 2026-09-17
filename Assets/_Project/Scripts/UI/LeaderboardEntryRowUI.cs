using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Cloud;
using RoyalSmash.Profile;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Component managing an individual leaderboard row item.
    /// Handles Rank styling (Top 3 Gold/Silver/Bronze), Avatar, Frame, Username, Level, and Local Player highlight.
    /// Follows Master Build Prompt Section 19.2.
    /// </summary>
    public class LeaderboardEntryRowUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _background;
        [SerializeField] private Outline _outline;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TextMeshProUGUI _usernameText;
        [SerializeField] private TextMeshProUGUI _levelText;

        [Header("Color Palettes")]
        [SerializeField] private Color _goldRankColor = new Color(1.0f, 0.84f, 0.0f);   // #FFD700
        [SerializeField] private Color _silverRankColor = new Color(0.78f, 0.82f, 0.88f); // #C7D1E0
        [SerializeField] private Color _bronzeRankColor = new Color(0.80f, 0.50f, 0.20f); // #CD7F32
        [SerializeField] private Color _defaultRankColor = new Color(0.65f, 0.70f, 0.80f);

        [SerializeField] private Color _localPlayerBg = new Color(0.18f, 0.26f, 0.42f, 0.95f);
        [SerializeField] private Color _normalRowBg = new Color(0.12f, 0.14f, 0.20f, 0.85f);
        [SerializeField] private Color _altRowBg = new Color(0.14f, 0.17f, 0.24f, 0.85f);

        private LeaderboardEntryData _data;
        public LeaderboardEntryData Data => _data;

        public void BindReferences(
            Image bg, Outline outline, TextMeshProUGUI rankTxt,
            Image avatarImg, Image frameImg, TextMeshProUGUI userTxt, TextMeshProUGUI lvlTxt)
        {
            _background = bg;
            _outline = outline;
            _rankText = rankTxt;
            _avatarImage = avatarImg;
            _frameImage = frameImg;
            _usernameText = userTxt;
            _levelText = lvlTxt;
        }

        public void Setup(LeaderboardEntryData data, ProfileConfig profileConfig = null)
        {
            _data = data;
            if (data == null) return;

            // 1. Rank Text & Styling
            if (_rankText != null)
            {
                _rankText.text = $"#{data.Rank}";
                if (data.Rank == 1) _rankText.color = _goldRankColor;
                else if (data.Rank == 2) _rankText.color = _silverRankColor;
                else if (data.Rank == 3) _rankText.color = _bronzeRankColor;
                else _rankText.color = _defaultRankColor;
            }

            // 2. Username
            if (_usernameText != null)
            {
                _usernameText.text = data.IsLocalPlayer ? $"{data.Username} (You)" : data.Username;
                _usernameText.color = data.IsLocalPlayer ? new Color(1.0f, 0.85f, 0.3f) : Color.white;
            }

            // 3. Level
            if (_levelText != null)
            {
                _levelText.text = $"Lvl {data.Level}";
            }

            // 4. Avatar & Frame
            if (profileConfig != null)
            {
                if (_avatarImage != null)
                {
                    var avSprite = profileConfig.GetAvatarSprite(data.AvatarIndex);
                    if (avSprite != null) _avatarImage.sprite = avSprite;
                }
                if (_frameImage != null)
                {
                    var frSprite = profileConfig.GetFrameSprite(data.FrameIndex);
                    if (frSprite != null)
                    {
                        _frameImage.sprite = frSprite;
                        _frameImage.gameObject.SetActive(true);
                    }
                    else
                    {
                        _frameImage.gameObject.SetActive(false);
                    }
                }
            }

            // 5. Background & Highlight
            if (_background != null)
            {
                if (data.IsLocalPlayer)
                {
                    _background.color = _localPlayerBg;
                    if (_outline != null) _outline.enabled = true;
                }
                else
                {
                    _background.color = (data.Rank % 2 == 0) ? _altRowBg : _normalRowBg;
                    if (_outline != null) _outline.enabled = false;
                }
            }
        }
    }
}
