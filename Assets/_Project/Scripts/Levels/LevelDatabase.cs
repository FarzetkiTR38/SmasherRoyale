using System;
using UnityEngine;

namespace RoyalSmash.Levels
{
    /// <summary>
    /// Central catalog holding all 30 LevelData assets for sequential progression.
    /// Follows Master Build Prompt Section 1.2 (Rule 9) and Section 35 (Phase 19).
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "RoyalSmash/Levels/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private LevelData[] _levels = new LevelData[30];

        public int TotalLevels => _levels != null ? _levels.Length : 0;

        public LevelData GetLevel(int levelId)
        {
            // LevelId is 1-based (1 to 30)
            int index = levelId - 1;
            if (_levels != null && index >= 0 && index < _levels.Length)
            {
                return _levels[index];
            }

            Debug.LogWarning($"[LevelDatabase] Level {levelId} requested out of range (1..{TotalLevels}).");
            return null;
        }

        public bool IsLastLevel(int levelId)
        {
            return levelId >= 30 || (_levels != null && levelId >= _levels.Length);
        }

        public void SetLevels(LevelData[] levels)
        {
            _levels = levels;
        }
    }
}
