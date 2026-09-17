using System;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Serializable data model representing an individual leaderboard entry.
    /// Follows Master Build Prompt Section 19.2 (Rank, Avatar, Username, Level).
    /// </summary>
    [Serializable]
    public class LeaderboardEntryData
    {
        public int Rank;
        public string PlayerId;
        public string Username;
        public int Level;
        public int AvatarIndex;
        public int FrameIndex;
        public bool IsLocalPlayer;

        public LeaderboardEntryData() { }

        public LeaderboardEntryData(int rank, string playerId, string username, int level, int avatarIndex, int frameIndex, bool isLocalPlayer = false)
        {
            Rank = rank;
            PlayerId = playerId;
            Username = username;
            Level = level;
            AvatarIndex = avatarIndex;
            FrameIndex = frameIndex;
            IsLocalPlayer = isLocalPlayer;
        }

        public LeaderboardEntryData Clone()
        {
            return new LeaderboardEntryData(Rank, PlayerId, Username, Level, AvatarIndex, FrameIndex, IsLocalPlayer);
        }
    }
}
