#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ESF.Core.PField.Editor
{
    public static class PFieldHelper
    {
        public static Color TypeSelectorButtonColor = new Color(0.3355426f, 0.1695888f, 0.9716981f);
        public const string PListInvalidNestingText = "Nested PList is only supported when container array is also a PList";
        public const string PListInvalidTypeText = "PList doesn't support classes derived from UnityEngine.Object class";

        public static string GetNiceTypeName(SerializedProperty prop)
        {
            string typeName = "Null (Select)";

            if (prop.managedReferenceValue != null)
            {
                int start = prop.type.IndexOf('<');
                int end = prop.type.IndexOf('>') - 1;
                typeName = prop.type.Substring(start + 1, end - start);
            }

            return typeName;
        }
    }
}
#endif