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

        // Lives
        public int Lives = 5;
        public long LifeTimerStartTicks = 0;

        // Active Skills
        public bool MegaBallUnlocked = false;
        public bool InfinityBallUnlocked = false;
        public int MegaBallUses = 3;
        public int InfinityBallUses = 3;

        // Settings
        public bool MusicEnabled = true;
        public bool SfxEnabled = true;
        public bool VibrationEnabled = true;

        // Metadata
        public long LastSaveTicks = 0;
    }
}
