using UnityEditor;
using UnityEngine;

namespace RoyalSmash.Editor
{
    [InitializeOnLoad]
    public static class AutoStartUnityMCP
    {
        static AutoStartUnityMCP()
        {
            EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
            EditorApplication.delayCall += () =>
            {
                try
                {
                    MCPForUnity.Editor.Windows.MCPForUnityEditorWindow.ShowWindow();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[AutoStartUnityMCP] Failed to open MCP window: " + ex.Message);
                }
            };
        }
    }
}
