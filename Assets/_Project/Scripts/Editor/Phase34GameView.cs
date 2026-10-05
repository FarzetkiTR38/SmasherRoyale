#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoyalSmash.EditorTools
{
    /// <summary>Explicit temporary portrait GameView sizes for repeatable visual checks.</summary>
    public static class Phase34GameView
    {
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public static string SetSize(int width, int height)
        {
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var viewType = assembly.GetType("UnityEditor.GameView");
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var view = EditorWindow.GetWindow(viewType);
            var sizes = sizesType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
            var group = sizesType.GetProperty("currentGroup", Instance).GetValue(sizes);
            var indexProperty = viewType.GetProperty("selectedSizeIndex", Instance);
            if (!SessionState.GetBool("Phase34.GameViewSaved", false))
            {
                SessionState.SetInt("Phase34.GameViewIndex", (int)indexProperty.GetValue(view));
                SessionState.SetInt("Phase34.GameViewCount", (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null));
                SessionState.SetBool("Phase34.GameViewSaved", true);
            }
            int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
            int selected = -1;
            for (int i = 0; i < count; i++)
            {
                var item = group.GetType().GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                if ((int)item.GetType().GetProperty("width", Instance).GetValue(item) == width &&
                    (int)item.GetType().GetProperty("height", Instance).GetValue(item) == height)
                { selected = i; break; }
            }
            if (selected < 0)
            {
                var sizeType = assembly.GetType("UnityEditor.GameViewSize");
                var kindType = assembly.GetType("UnityEditor.GameViewSizeType");
                var item = Activator.CreateInstance(sizeType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new object[] { Enum.Parse(kindType, "FixedResolution"), width, height, "Phase34 " + width + "x" + height }, null);
                group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { item });
                selected = count;
            }
            indexProperty.SetValue(view, selected);
            view.Repaint();
            return "GameView selected " + width + "x" + height;
        }

        public static string Restore()
        {
            if (!SessionState.GetBool("Phase34.GameViewSaved", false)) return "No temporary GameView state.";
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var viewType = assembly.GetType("UnityEditor.GameView");
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var view = EditorWindow.GetWindow(viewType);
            var sizes = sizesType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
            var group = sizesType.GetProperty("currentGroup", Instance).GetValue(sizes);
            viewType.GetProperty("selectedSizeIndex", Instance).SetValue(view, SessionState.GetInt("Phase34.GameViewIndex", 0));
            int originalCount = SessionState.GetInt("Phase34.GameViewCount", 0);
            int count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
            for (int i = count - 1; i >= originalCount; i--)
                group.GetType().GetMethod("RemoveCustomSize").Invoke(group, new object[] { i });
            SessionState.SetBool("Phase34.GameViewSaved", false);
            view.Repaint();
            return "Original GameView selection restored.";
        }
    }
}
#endif
