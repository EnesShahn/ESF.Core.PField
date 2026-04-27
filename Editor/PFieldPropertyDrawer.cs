#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace ESF.Core.PField.Editor
{
    [CustomPropertyDrawer(typeof(PFieldAttribute), true)]
    public class PFieldPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty serializedProperty;
        private Type propType;
        private Type[] propTypes;
        private TypeSelectorGenericMenu typeSelectorGenericMenu;
        private bool initialized;

        private bool isInvalidAttributeUse;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            Init(property);

            if (isInvalidAttributeUse)
            {
                rect.height = EditorGUIUtility.singleLineHeight * 2;
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.HelpBox(rect, "Please do NOT use PFieldAttribute on a PList<> type field", MessageType.Error);
                return;
            }

            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
            DrawProperty(rect);
            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (isInvalidAttributeUse)
            {
                return EditorGUIUtility.singleLineHeight * 2.5f;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property, property.isExpanded) + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private void Init(SerializedProperty property)
        {
            if (initialized) return;
            initialized = true;

            isInvalidAttributeUse = property.GetPropertyFieldType().GetGenericTypeDefinitionIfGeneric() == typeof(PList<>);
            if (isInvalidAttributeUse) return;

            serializedProperty = property;
            propType = property.GetPropertyFieldType();
            propTypes = propType.GetChildTypes(!propType.IsAbstract);

            typeSelectorGenericMenu = new TypeSelectorGenericMenu(propTypes, OnGenericMenuTypeSelected);
        }

        private void DrawProperty(Rect rect)
        {
            var propRect = new Rect(rect);
            object managedRefVal = serializedProperty.managedReferenceValue;

            int start = serializedProperty.type.IndexOf('<');
            int end = serializedProperty.type.IndexOf('>') - 1;
            string typeName = managedRefVal != null ? serializedProperty.type.Substring(start + 1, end - start) : "Null (Select)";

            DrawTypeSelectorButton(propRect, typeName);
            EditorGUI.PropertyField(propRect, serializedProperty, new GUIContent(serializedProperty.displayName), true);
        }
        private void DrawTypeSelectorButton(Rect rect, string label)
        {
            var buttonRect = new Rect(rect);
            buttonRect.y += 1;
            buttonRect.x += EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;
            buttonRect.width -= EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;
            buttonRect.height = EditorGUIUtility.singleLineHeight;

            var storedColor = GUI.backgroundColor;

            GUI.backgroundColor = PFieldHelper.TypeSelectorButtonColor;

            if (GUI.Button(buttonRect, label))
            {
                typeSelectorGenericMenu.ShowAsContext();
            }

            GUI.backgroundColor = storedColor;
        }

        private void OnGenericMenuTypeSelected(Type type)
        {
            object obj = null;
            if (type != null)
            {
                obj = Activator.CreateInstance(type);
            }

            serializedProperty.managedReferenceValue = obj;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }
        private void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            var propCopy = property.Copy();
            menu.AddItem(new GUIContent("Copy"), false, () =>
            {
                if (ReferenceEquals(propCopy.managedReferenceValue, null)) return;

                Type propObjectType = propCopy.managedReferenceValue.GetType();
                string serializedObject = JsonUtility.ToJson(propCopy.GetObjectValue());
                string data = $"PField::{propObjectType.AssemblyQualifiedName}::{serializedObject}";

                EditorGUIUtility.systemCopyBuffer = data;
            });
            menu.AddItem(new GUIContent("Paste"), false, () =>
            {
                try
                {
                    string data = EditorGUIUtility.systemCopyBuffer;
                    if (!data.StartsWith("PField")) return;
                    string[] dataTokens = data.Split("::");
                    Type objectType = Type.GetType(dataTokens[1], true, true);
                    object deserializedObject = JsonUtility.FromJson(dataTokens[2], objectType);

                    propCopy.managedReferenceValue = deserializedObject;
                    propCopy.serializedObject.ApplyModifiedProperties();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
        }
    }
}
#endif