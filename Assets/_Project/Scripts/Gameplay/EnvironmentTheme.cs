using System;
using UnityEngine;

namespace RoyalSmash.Gameplay
{
    /// <summary>
    /// Configuration data for a visual environment theme.
    /// Controls sky colors, ambient lighting, directional sun color/intensity, and depth fog.
    /// Follows Master Build Prompt Section 25 & Sections 297-298 of Full Documentation.
    /// </summary>
    [Serializable]
    public class EnvironmentTheme
    {
        [Header("Theme Identification")]
        public string ThemeName = "Castle Courtyard";
        public int StartLevel = 1;
        public int EndLevel = 10;

        [Header("Lighting")]
        public Color SunLightColor = new Color(1f, 0.96f, 0.88f);
        public float SunLightIntensity = 1.15f;
        public Vector3 SunEulerAngles = new Vector3(50f, -30f, 0f);

        [Header("Sky & Background")]
        public Color SkyTopColor = new Color(0.35f, 0.65f, 0.95f);
        public Color SkyHorizonColor = new Color(0.75f, 0.88f, 0.98f);
        public Color GroundColor = new Color(0.28f, 0.52f, 0.22f);

        [Header("Atmospheric Fog")]
        public bool EnableFog = true;
        public Color FogColor = new Color(0.68f, 0.82f, 0.95f);
        public float FogStartDistance = 18f;
        public float FogEndDistance = 45f;
    }
}
