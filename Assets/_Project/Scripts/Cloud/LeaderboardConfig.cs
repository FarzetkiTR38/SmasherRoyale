using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoyalSmash.Cloud
{
    /// <summary>
    /// Configuration data for Leaderboard display, default competitors, and caching policies.
    /// Follows Master Build Prompt Section 19 & Section 35 (Phase 15).
    /// </summary>
    [CreateAssetMenu(fileName = "LeaderboardConfig", menuName = "RoyalSmash/Config/LeaderboardConfig")]
    public class LeaderboardConfig : ScriptableObject
    {
        [Header("Leaderboard Limits")]
        [SerializeField] private int _maxDisplayedEntries = 1000;
        [SerializeField] private int _defaultMockCompetitorCount = 35;
        [SerializeField] private float _cacheExpiryMinutes = 30f;

        [Header("Simulation")]
        [SerializeField] private bool _enableSimulatedNetwork = true;
        [SerializeField] private float _simulatedLatencySeconds = 0.3f;

        public int MaxDisplayedEntries => _maxDisplayedEntries;
        public int DefaultMockCompetitorCount => _defaultMockCompetitorCount;
        public float CacheExpiryMinutes => _cacheExpiryMinutes;
        public bool EnableSimulatedNetwork { get => _enableSimulatedNetwork; set => _enableSimulatedNetwork = value; }
        public float SimulatedLatencySeconds => _simulatedLatencySeconds;

        private static readonly string[] RoyalNames = new string[]
        {
            "King Arthur", "Sir Lancelot", "Queen Guinevere", "Lord Percival",
            "Lady Marian", "Baron Cedric", "Duchess Eleanor", "Knight Roland",
            "Archmage Eldrin", "Count Valerius", "Princess Rosamund", "Sir Galahad",
            "Duke Tristan", "Lady Genevieve", "Captain Drake", "Master Robin",
            "High Lord Alistair", "Lady Seraphina", "Sir Bedivere", "Dame Joan",
            "Prince Cassian", "Viscount William", "Paladin Gareth", "Lady Morgana",
            "Lord Kenneth", "Sir Kay", "Duchess Aurelia", "Grand Master Lucian",
            "Baroness Beatrice", "Ranger Thorne", "Knight Hector", "Lady Vivienne",
            "Archon Keith", "Sir Brian", "Queen Isabella"
        };

        /// <summary>
        /// Generates a set of default royal realm competitors sorted for mock offline/online leaderboards.
        /// </summary>
        public List<LeaderboardEntryData> GenerateDefaultCompetitors()
        {
            var list = new List<LeaderboardEntryData>();
            var rnd = new System.Random(42); // Deterministic seed

            for (int i = 0; i < _defaultMockCompetitorCount && i < RoyalNames.Length; i++)
            {
                // Top competitors range realistically from level 29 down to level 2
                int countMinusOne = Mathf.Max(1, _defaultMockCompetitorCount - 1);
                int level = Mathf.Clamp(29 - (i * 27 / countMinusOne), 2, 29);
                int avatar = rnd.Next(0, 8);
                int frame = rnd.Next(0, 4);
                string id = $"bot_{i + 1:D3}";

                list.Add(new LeaderboardEntryData(
                    rank: i + 1,
                    playerId: id,
                    username: RoyalNames[i],
                    level: level,
                    avatarIndex: avatar,
                    frameIndex: frame,
                    isLocalPlayer: false
                ));
            }

            return list;
        }
    }
}
