using System;
using UnityEngine;

namespace RoyalSmash.Utils
{
    /// <summary>
    /// Provides persistent unique device identification across application runs, uninstalls, and reinstalls.
    /// Uses SystemInfo.deviceUniqueIdentifier on mobile platforms (Android Settings.Secure.ANDROID_ID, iOS vendor ID).
    /// Supports test overrides for multi-device simulation.
    /// Follows Master Build Prompt Section 28 & casual mobile data persistence patterns.
    /// </summary>
    public static class DeviceIdProvider
    {
        private static string _overrideDeviceId = null;
        public static bool HasOverride => !string.IsNullOrEmpty(_overrideDeviceId);

        /// <summary>
        /// Allows test suites and simulated devices to set a custom Device ID.
        /// Pass null or empty to reset to system default.
        /// </summary>
        public static void SetOverrideDeviceId(string id)
        {
            _overrideDeviceId = id;
            if (!string.IsNullOrEmpty(id))
            {
                Debug.Log($"[DeviceIdProvider] Override Device ID active: '{id}'");
            }
            else
            {
                Debug.Log("[DeviceIdProvider] Reset to hardware Device ID.");
            }
        }

        /// <summary>
        /// Gets the unique persistent identifier for this physical or simulated device.
        /// Survives application uninstall and reinstall on mobile devices.
        /// </summary>
        public static string GetDeviceId()
        {
            if (!string.IsNullOrEmpty(_overrideDeviceId))
            {
                return _overrideDeviceId;
            }

            string id = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(id) || id == SystemInfo.unsupportedIdentifier)
            {
                // Fallback for headless environments or unsupported platforms
                id = "dev_fallback_" + Application.platform.ToString().ToLower();
            }

            return id;
        }
    }
}
