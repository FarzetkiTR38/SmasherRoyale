using System;
using UnityEditor;
using UnityEngine;
using RoyalSmash.Cloud;

namespace RoyalSmash.EditorTools
{
    public static class UGSTestRunner
    {
        private static bool _pendingTest = false;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && _pendingTest)
            {
                _pendingTest = false;
                var go = new GameObject("UGS_Live_Tester");
                var tester = go.AddComponent<UGSRuntimeTester>();
                tester.runOnStart = true;
                Debug.Log("[UGSTestRunner] Entered Play Mode. Launched UGSRuntimeTester.");
            }
        }

        [MenuItem("RoyalSmash/Live Test - UGS Cloud & Leaderboard", false, 50)]
        public static void RunLiveUGSTest()
        {
            if (EditorApplication.isPlaying)
            {
                var existing = UnityEngine.Object.FindFirstObjectByType<UGSRuntimeTester>();
                if (existing == null)
                {
                    var go = new GameObject("UGS_Live_Tester");
                    existing = go.AddComponent<UGSRuntimeTester>();
                }
                _ = existing.ExecuteLiveTestAsync();
            }
            else
            {
                Debug.Log("[UGSTestRunner] Entering Play Mode to run UGS live test...");
                _pendingTest = true;
                EditorApplication.isPlaying = true;
            }
        }
    }
}
