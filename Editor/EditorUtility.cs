#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

namespace ESF.Core.PField.Editor
{
    public static class EditorUtility
    {
        private static bool initializeCurrentEditorWindowMetadata;
        private static PropertyInfo current;
        private static FieldInfo m_ActualView;

        public static EditorWindow GetCurrentEditorWindow()
        {
            if (EditorUtility.initializeCurrentEditorWindowMetadata == false)
            {
                EditorUtility.initializeCurrentEditorWindowMetadata = true;
                EditorUtility.LazyInitializeCurrentEditorWindowMetadata();
            }

            if (EditorUtility.current == null)
                return null;

            object guiView = EditorUtility.current.GetValue(null, null);

            if (guiView != null)
                return EditorUtility.m_ActualView.GetValue(guiView) as EditorWindow;
            return null;
        }

        private static void LazyInitializeCurrentEditorWindowMetadata()
        {
            Type GUIViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
            if (GUIViewType != null)
            {
                EditorUtility.current = GUIViewType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);

                Type HostViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.HostView");
                if (HostViewType != null)
                    EditorUtility.m_ActualView = HostViewType.GetField("m_ActualView", BindingFlags.NonPublic | BindingFlags.Instance);

                if (EditorUtility.m_ActualView == null)
                    EditorUtility.current = null;
            }
        }
    }
}
#endif