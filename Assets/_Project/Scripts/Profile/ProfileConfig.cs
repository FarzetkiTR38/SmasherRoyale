using System;
using UnityEngine;

namespace RoyalSmash.Profile
{
    [Serializable]
    public class AvatarData
    {
        public int Id;
        public string Name;
        public Color ThemeColor = Color.white;
        public Sprite Icon;
    }

    [Serializable]
    public class FrameData
    {
        public int Id;
        public string Name;
        public Color BorderColor = Color.yellow;
        public Sprite BorderSprite;
    }

    /// <summary>
    /// ScriptableObject defining the 8 default avatars and 4 default frames (Section 18).
    /// All avatars and frames are available from start without gating or unlock requirements.
    /// </summary>
    [CreateAssetMenu(fileName = "ProfileConfig", menuName = "RoyalSmash/Profile/ProfileConfig")]
    public class ProfileConfig : ScriptableObject
    {
        [Header("Default Avatars (8 Available)")]
        [SerializeField] private AvatarData[] _avatars = new AvatarData[8];

        [Header("Default Frames (4 Available)")]
        [SerializeField] private FrameData[] _frames = new FrameData[4];

        public AvatarData[] Avatars => _avatars;
        public FrameData[] Frames => _frames;

        public AvatarData GetAvatar(int id)
        {
            if (_avatars != null && id >= 0 && id < _avatars.Length)
            {
                return _avatars[id];
            }
            return _avatars != null && _avatars.Length > 0 ? _avatars[0] : null;
        }

        public FrameData GetFrame(int id)
        {
            if (_frames != null && id >= 0 && id < _frames.Length)
            {
                return _frames[id];
            }
            return _frames != null && _frames.Length > 0 ? _frames[0] : null;
        }

        public Sprite GetAvatarSprite(int id)
        {
            return GetAvatar(id)?.Icon;
        }

        public Sprite GetFrameSprite(int id)
        {
            return GetFrame(id)?.BorderSprite;
        }
    }
}
