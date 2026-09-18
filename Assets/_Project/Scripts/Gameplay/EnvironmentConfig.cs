using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// ScriptableObject storing progression environment themes across the 30 MVP levels.
    /// Follows Master Build Prompt Section 25 & Sections 297-298.
    /// </summary>
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "RoyalSmash/Config/Environment Config")]
    public class EnvironmentConfig : ScriptableObject
    {
        [SerializeField] private EnvironmentTheme[] _themes;

        public EnvironmentTheme[] Themes => _themes;

        /// <summary>
        /// Returns the appropriate environment theme for the given level number (1..30).
        /// </summary>
        public EnvironmentTheme GetThemeForLevel(int level)
        {
            if (_themes == null || _themes.Length == 0)
            {
                return GetDefaultTheme();
            }

            foreach (var t in _themes)
            {
                if (level >= t.StartLevel && level <= t.EndLevel)
                {
                    return t;
                }
            }

            // If beyond last theme, return final theme
            return _themes[_themes.Length - 1];
        }

        public static EnvironmentTheme GetDefaultTheme()
        {
            return new EnvironmentTheme
            {
                ThemeName = "Castle Courtyard (Default)",
                StartLevel = 1,
                EndLevel = 10,
                SunLightColor = new Color(1f, 0.96f, 0.88f),
                SunLightIntensity = 1.15f,
                SkyTopColor = new Color(0.35f, 0.65f, 0.95f),
                SkyHorizonColor = new Color(0.75f, 0.88f, 0.98f),
                GroundColor = new Color(0.28f, 0.52f, 0.22f),
                EnableFog = true,
                FogColor = new Color(0.68f, 0.82f, 0.95f),
                FogStartDistance = 18f,
                FogEndDistance = 45f
            };
        }
    }
}
