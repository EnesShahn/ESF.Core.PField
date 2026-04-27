#if UNITY_EDITOR
using System.Reflection;
using UnityEditorInternal;

namespace ESF.Core.PField.Editor
{
    public static class ReorderableListExtensions
    {
        private static MethodInfo InvalidateCacheRecursiveMethodInfo;

        static ReorderableListExtensions()
        {
            InvalidateCacheRecursiveMethodInfo = typeof(ReorderableList).GetMethod("InvalidateCacheRecursive", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void InvalidateCacheRecursive(this ReorderableList list)
        {
            if (list == null) return;
            InvalidateCacheRecursiveMethodInfo.Invoke(list, null);
        }
    }
}
#endif