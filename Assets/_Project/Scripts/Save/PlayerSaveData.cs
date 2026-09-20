using System;

namespace RoyalSmash.Save
{
    /// <summary>
    /// Serializable player save data model.
    /// Follows Master Build Prompt and Save System Skill architecture.
    /// Serialized to JSON in persistent local storage.
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        // Versioning
        public int SaveVersion = 1;

        // Progression
        public int CurrentLevel = 1;
        public bool TutorialCompleted = false;

        // Economy
        public int Gold = 1000;
        public System.Collections.Generic.List<int> RewardedLevelIds = new System.Collections.Generic.List<int>();
        public bool NoAdsPurchased = false;

        // Lives
        public int Lives = 5;
        public long LifeTimerStartTicks = 0;
        public bool WasInActiveLevel = false;
        public int ActiveLevelId = 1;

        // Active Skills
        public bool MegaBallUnlocked = false;
        public bool InfinityBallUnlocked = false;
        public int MegaBallUses = 3;
        public int InfinityBallUses = 3;

        // Settings
        public bool MusicEnabled = true;
        public bool SfxEnabled = true;
        public bool VibrationEnabled = true;

        // Profile (Section 18)
        public string Username = "Player";
        public int AvatarId = 0;
        public int FrameId = 0;
        public bool UsernamePrompted = false;

        // Statistics (Section 18.1)
        public int TotalShotsFired = 0;
        public int TotalWins = 0;
        public int CompletedLevelsCount = 0;

        // Metadata
        public long LastSaveTicks = 0;

        // Aliases & Utilities
        public int AvatarIndex { get => AvatarId; set => AvatarId = value; }
        public int FrameIndex { get => FrameId; set => FrameId = value; }

        public PlayerSaveData Clone()
        {
            var clone = (PlayerSaveData)this.MemberwiseClone();
            if (this.RewardedLevelIds != null)
            {
                clone.RewardedLevelIds = new System.Collections.Generic.List<int>(this.RewardedLevelIds);
            }
            return clone;
        }
    }
}
