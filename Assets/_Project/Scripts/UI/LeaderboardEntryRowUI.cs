using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoyalSmash.Cloud;
using RoyalSmash.Profile;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Component managing an individual leaderboard row item.
    /// Handles Rank styling (Top 3 Gold/Silver/Bronze cards), Avatar, Frame, Username, Stage (Aşama), and Local Player highlight.
    /// Follows Master Build Prompt Section 19.2 and target visual specification.
    /// </summary>
    public class LeaderboardEntryRowUI : MonoBehaviour
    {
        [Header("Card Sprites")]
        [SerializeField] private Sprite _goldCardSprite;
        [SerializeField] private Sprite _silverCardSprite;
        [SerializeField] private Sprite _bronzeCardSprite;
        [SerializeField] private Sprite _normalBlueCardSprite;

        [Header("UI Elements")]
        [SerializeField] private Image _background;
        [SerializeField] private Outline _outline;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TextMeshProUGUI _usernameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _stageLabelText;

        [Header("Color Palettes")]
        [SerializeField] private Color _goldRankColor = new Color(1.0f, 0.95f, 0.70f);   // Bright Gold
        [SerializeField] private Color _silverRankColor = new Color(0.90f, 0.95f, 1.0f);  // Platinum Silver
        [SerializeField] private Color _bronzeRankColor = new Color(1.0f, 0.80f, 0.60f);  // Bronze Amber
        [SerializeField] private Color _defaultRankColor = new Color(0.55f, 0.82f, 1.0f); // Bright Cyber Blue

        [SerializeField] private Color _localPlayerBg = new Color(0.18f, 0.26f, 0.42f, 0.95f);
        [SerializeField] private Color _normalRowBg = new Color(0.12f, 0.14f, 0.20f, 0.85f);
        [SerializeField] private Color _altRowBg = new Color(0.14f, 0.17f, 0.24f, 0.85f);

        private LeaderboardEntryData _data;
        public LeaderboardEntryData Data => _data;

        public void BindReferences(
            Image bg, Outline outline, TextMeshProUGUI rankTxt,
            Image avatarImg, Image frameImg, TextMeshProUGUI userTxt, TextMeshProUGUI lvlTxt,
            TextMeshProUGUI stageLabel = null)
        {
            _background = bg;
            _outline = outline;
            _rankText = rankTxt;
            _avatarImage = avatarImg;
            _frameImage = frameImg;
            _usernameText = userTxt;
            _levelText = lvlTxt;
            _stageLabelText = stageLabel;
        }

        public void SetCardSprites(Sprite gold, Sprite silver, Sprite bronze, Sprite normal)
        {
            _goldCardSprite = gold;
            _silverCardSprite = silver;
            _bronzeCardSprite = bronze;
            _normalBlueCardSprite = normal;
        }

        public void Setup(LeaderboardEntryData data, ProfileConfig profileConfig = null)
        {
            _data = data;
            if (data == null) return;

            // 1. Card Background & Rank Text
            if (data.Rank == 1)
            {
                if (_background != null && _goldCardSprite != null)
                {
                    _background.sprite = _goldCardSprite;
                    _background.color = Color.white;
                }
                if (_rankText != null)
                {
                    _rankText.text = "1";
                    _rankText.color = _goldRankColor;
                }
            }
            else if (data.Rank == 2)
            {
                if (_background != null && _silverCardSprite != null)
                {
                    _background.sprite = _silverCardSprite;
                    _background.color = Color.white;
                }
                if (_rankText != null)
                {
                    _rankText.text = "2";
                    _rankText.color = _silverRankColor;
                }
            }
            else if (data.Rank == 3)
            {
                if (_background != null && _bronzeCardSprite != null)
                {
                    _background.sprite = _bronzeCardSprite;
                    _background.color = Color.white;
                }
                if (_rankText != null)
                {
                    _rankText.text = "3";
                    _rankText.color = _bronzeRankColor;
                }
            }
            else
            {
                if (_background != null)
                {
                    if (_normalBlueCardSprite != null)
                    {
                        _background.sprite = _normalBlueCardSprite;
                        _background.color = Color.white;
                    }
                    else
                    {
                        _background.color = (data.Rank % 2 == 0) ? _altRowBg : _normalRowBg;
                    }
                }
                if (_rankText != null)
                {
                    _rankText.text = data.Rank.ToString();
                    _rankText.color = _defaultRankColor;
                }
            }

            // 2. Username
            if (_usernameText != null)
            {
                _usernameText.text = data.IsLocalPlayer ? $"{data.Username} (Sen)" : data.Username;
                _usernameText.color = data.IsLocalPlayer ? new Color(1.0f, 0.90f, 0.4f) : Color.white;
            }

            // 3. Stage / Level Text
            if (_stageLabelText != null)
            {
                _stageLabelText.text = "Level";
                if (data.Rank == 1) _stageLabelText.color = new Color(1.0f, 0.90f, 0.65f);
                else if (data.Rank == 2) _stageLabelText.color = new Color(0.90f, 0.95f, 1.0f);
                else if (data.Rank == 3) _stageLabelText.color = new Color(1.0f, 0.85f, 0.70f);
                else _stageLabelText.color = new Color(0.75f, 0.90f, 1.0f);
            }

            if (_levelText != null)
            {
                int displayLevel = Mathf.Clamp(data.Level, 1, 30);
                _levelText.text = displayLevel.ToString();
                if (data.Rank == 1) _levelText.color = new Color(1.0f, 0.88f, 0.3f);
                else if (data.Rank == 2) _levelText.color = Color.white;
                else if (data.Rank == 3) _levelText.color = new Color(1.0f, 0.75f, 0.5f);
                else _levelText.color = new Color(0.85f, 0.92f, 1.0f);
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

            // 5. Local Player Highlight Outline
            if (_outline != null)
            {
                _outline.enabled = data.IsLocalPlayer;
            }
        }
    }
}
