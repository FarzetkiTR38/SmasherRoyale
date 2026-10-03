using System;
using System.Collections.Generic;
using RoyalSmash.Save;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Interface for cloud sync and remote leaderboard data providers.
    /// Supports "Latest local progress wins" philosophy per Master Build Prompt Section 28.
    /// </summary>
    public interface ICloudSyncProvider
    {
        /// <summary>
        /// True if connection to the cloud backend is currently active and reachable.
        /// </summary>
        bool IsOnline { get; }

        /// <summary>
        /// Allows toggling online state for offline testing or flight-mode simulation.
        /// </summary>
        void SetOnlineState(bool isOnline);

        /// <summary>
        /// Pushes local player progress to cloud backend adhering to "Latest local progress wins".
        /// </summary>
        void SyncProgress(PlayerSaveData localData, Action<bool, string> onComplete = null);

        /// <summary>
        /// Fetches the current global leaderboard entries from cloud storage.
        /// </summary>
        void FetchLeaderboard(Action<bool, List<LeaderboardEntryData>, string> onComplete);

        /// <summary>
        /// Retrieves the saved player progress from the cloud for the specified Device ID.
        /// Used during app reinstall or cross-session recovery.
        /// </summary>
        void LoadProgress(string deviceId, Action<bool, PlayerSaveData, string> onComplete);
    }
}
