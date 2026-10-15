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
                    var mcpType = System.Type.GetType("MCPForUnity.Editor.Windows.MCPForUnityEditorWindow, MCPForUnity.Editor");
                    if (mcpType != null)
                    {
                        var showMethod = mcpType.GetMethod("ShowWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        showMethod?.Invoke(null, null);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[AutoStartUnityMCP] Failed to open MCP window: " + ex.Message);
                }
            };
        }
    }
}
