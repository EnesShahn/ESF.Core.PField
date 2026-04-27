#if UNITY_EDITOR
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

namespace ESF.Core.PField.Editor
{
    [CustomPropertyDrawer(typeof(PList<>), true)]
    public class PListPropertyDrawer : PropertyDrawer
    {
        private Dictionary<string, PListDrawer> drawersCache = new Dictionary<string, PListDrawer>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!drawersCache.ContainsKey(property.propertyPath))
                drawersCache[property.propertyPath] = new PListDrawer(property.Copy());
            var drawer = drawersCache[property.propertyPath];
            drawer.OnGUI(position);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!drawersCache.ContainsKey(property.propertyPath))
                drawersCache[property.propertyPath] = new PListDrawer(property.Copy());
            var drawer = drawersCache[property.propertyPath];
            return drawer.GetHeight();
        }
    }

    public class PListDrawer
    {
        private SerializedProperty serializedProperty;
        private SerializedProperty listItemsSP;
        private object listItemsObj;
        private Type listItemType;
        private Type[] listItemTypes;
        private TypeSelectorGenericMenu typeSelectorGenericMenu;

        private int clickedButtonItemIndex;
        private int previousItemCount;
        private int rightClickedItemIndex;

        private bool isInvalidNesting;
        private bool isInvalidType;

        Dictionary<string, float> cachedHeights = new Dictionary<string, float>();

        public PListDrawer(SerializedProperty property)
        {
            serializedProperty = property.Copy();
            listItemsObj = serializedProperty.GetObjectValue();

            listItemsSP = serializedProperty.FindPropertyRelative("_items");

            listItemType = listItemsObj.GetType().GetGenericArguments()[0];
            isInvalidType = listItemType.IsAssignableFrom(typeof(UnityEngine.Object));
            if (isInvalidType) return;

            listItemTypes = listItemType.GetChildTypes(!listItemType.IsAbstract);
            isInvalidNesting = serializedProperty.IsAnyArrayAncestorNotOfType(typeof(PList<>));

            typeSelectorGenericMenu = new TypeSelectorGenericMenu(listItemTypes, OnGenericMenuTypeSelected);

            previousItemCount = listItemsSP.arraySize;
        }

        public void OnGUI(Rect rect)
        {
            serializedProperty.isExpanded = true; // Always make PList Expanded (array is part of PList)

            if (isInvalidNesting)
            {
                rect.height = EditorGUIUtility.singleLineHeight * 2;
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.HelpBox(rect, PFieldHelper.PListInvalidNestingText, MessageType.Error);
                return;
            }

            if (isInvalidType)
            {
                rect.height = EditorGUIUtility.singleLineHeight * 2;
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.HelpBox(rect, PFieldHelper.PListInvalidTypeText, MessageType.Error);
                return;
            }

            if (listItemsSP.isExpanded)
            {
                CheckButtonsInput(rect);
            }

            EditorGUI.PropertyField(rect, listItemsSP, new GUIContent(serializedProperty.displayName), true);

            CheckAndAdjustNewlyAddedItems();

            if (listItemsSP.isExpanded)
            {
                DrawVisualButtons(rect);
            }

            cachedHeights.Clear();
        }

        private void CheckAndAdjustNewlyAddedItems()
        {
            int currentItemCount = listItemsSP.arraySize;

            if (currentItemCount != previousItemCount)
            {
                if (currentItemCount > previousItemCount)
                {
                    int addedCount = (int)MathF.Abs(previousItemCount - currentItemCount);
                    bool ignoreNextIfs = false;

                    if (previousItemCount == 0 && listItemTypes.Length == 1) // If no element then set new items value to a new ListItemType instance since there is no other option
                    {
                        for (int i = 0; i < addedCount; i++)
                        {
                            var newItem = listItemsSP.GetArrayElementAtIndex(i);
                            object obj = Activator.CreateInstance(listItemType);
                            newItem.managedReferenceValue = obj;
                        }

                        ignoreNextIfs = true;
                    }

                    if (!ignoreNextIfs && previousItemCount != 0 && rightClickedItemIndex != -1) // If a new item was added and it is potentially one created from duplicate action
                    {
                        if (rightClickedItemIndex + 1 < listItemsSP.arraySize)
                        {
                            var duplicatedItem = listItemsSP.GetArrayElementAtIndex(rightClickedItemIndex);
                            var newItem = listItemsSP.GetArrayElementAtIndex(rightClickedItemIndex + 1);
                            if (duplicatedItem.managedReferenceValue == newItem.managedReferenceValue) // Check whether it was actual duplicated items
                            {
                                string serializedObject = JsonUtility.ToJson(duplicatedItem.managedReferenceValue);
                                newItem.managedReferenceValue = JsonUtility.FromJson(serializedObject, duplicatedItem.managedReferenceValue.GetType());
                                ignoreNextIfs = true;
                            }
                        }
                    }

                    if (!ignoreNextIfs && previousItemCount != 0)
                    {
                        var lastItemBeforeAddition = listItemsSP.GetArrayElementAtIndex(listItemsSP.arraySize - addedCount - 1);
                        if (lastItemBeforeAddition.managedReferenceValue != null)
                        {
                            string lastItemSerialized = JsonUtility.ToJson(lastItemBeforeAddition.managedReferenceValue);
                            for (int i = 0; i < addedCount; i++)
                            {
                                var item = listItemsSP.GetArrayElementAtIndex(listItemsSP.arraySize - 1 - i);
                                item.managedReferenceValue = JsonUtility.FromJson(lastItemSerialized, lastItemBeforeAddition.managedReferenceValue.GetType());
                                item.isExpanded = lastItemBeforeAddition.isExpanded;
                            }

                            ignoreNextIfs = true;
                        }
                    }

                    rightClickedItemIndex = -1;
                }

                previousItemCount = currentItemCount;
                serializedProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        //Checks if user clicks on any of the buttons OR whether or not the user right clicked on an element to duplicate an item
        private void CheckButtonsInput(Rect rect)
        {
            var buttonRect = rect;
            buttonRect.width = EditorGUIUtility.labelWidth * 1.2f;
            buttonRect.x += rect.width - buttonRect.width - 6;
            buttonRect.y += EditorGUIUtility.singleLineHeight + 7;
            buttonRect.height = EditorGUIUtility.singleLineHeight;

            var headerRect = rect;
            headerRect.y += EditorGUIUtility.singleLineHeight + 7;
            headerRect.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            clickedButtonItemIndex = -1;
            var e = Event.current;
            for (int i = 0; i < listItemsSP.arraySize; i++)
            {
                if (e.type == EventType.MouseDown)
                {
                    if ((e.button == 0 || e.button == 1 || e.button == 2) && buttonRect.Contains(e.mousePosition))
                    {
                        clickedButtonItemIndex = i;
                        typeSelectorGenericMenu.ShowAsContext();
                    }
                    else if (e.button == 1 && headerRect.Contains(e.mousePosition))
                    {
                        rightClickedItemIndex = i;
                    }
                }

                var item = listItemsSP.GetArrayElementAtIndex(i);
                float yIncrement = GetListItemHeight(item);
                headerRect.y += yIncrement;
                buttonRect.y += yIncrement;
            }
        }

        private void DrawVisualButtons(Rect rect)
        {
            var buttonRect = rect;
            buttonRect.width = EditorGUIUtility.labelWidth * 1.2f;
            buttonRect.x += rect.width - buttonRect.width - 6;
            buttonRect.y += EditorGUIUtility.singleLineHeight + 5 + EditorGUIUtility.standardVerticalSpacing;
            buttonRect.height = EditorGUIUtility.singleLineHeight;

            var storedColor = GUI.backgroundColor;
            GUI.backgroundColor = PFieldHelper.TypeSelectorButtonColor;

            for (int i = 0; i < listItemsSP.arraySize; i++)
            {
                var item = listItemsSP.GetArrayElementAtIndex(i);
                string typeName = PFieldHelper.GetNiceTypeName(item);
                GUI.Button(buttonRect, typeName);

                float yIncrement = GetListItemHeight(item) + EditorGUIUtility.standardVerticalSpacing;
                buttonRect.y += yIncrement;
            }

            GUI.backgroundColor = storedColor;
        }

        //Lazy cache heights each OnGUI call
        private float GetListItemHeight(SerializedProperty prop)
        {
            if (!prop.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (!cachedHeights.ContainsKey(prop.propertyPath))
            {
                bool storedExpand = prop.isExpanded;
                prop.isExpanded = true;
                cachedHeights[prop.propertyPath] = EditorGUI.GetPropertyHeight(prop, true);
                prop.isExpanded = storedExpand;
            }

            return cachedHeights[prop.propertyPath];
        }

        public float GetHeight()
        {
            if (isInvalidNesting || isInvalidType)
            {
                return EditorGUIUtility.singleLineHeight * 2.5f;
            }

            if (listItemsSP.arraySize == 0)
            {
                return EditorGUI.GetPropertyHeight(listItemsSP, true);
            }

            float totalHeight = EditorGUIUtility.singleLineHeight; // Header

            if (listItemsSP.isExpanded)
            {
                totalHeight += 5 + EditorGUIUtility.standardVerticalSpacing; // List Start Spacing

                for (int i = 0; i < listItemsSP.arraySize; i++)
                {
                    var item = listItemsSP.GetArrayElementAtIndex(i);
                    totalHeight += GetListItemHeight(item) + EditorGUIUtility.standardVerticalSpacing;
                }

                totalHeight += 20 + EditorGUIUtility.standardVerticalSpacing + 1; // List Add/Remove buttons
            }

            return totalHeight;
        }

        private void OnGenericMenuTypeSelected(Type type)
        {
            object obj = null;
            if (type != null)
            {
                obj = Activator.CreateInstance(type);
            }

            listItemsSP.GetArrayElementAtIndex(clickedButtonItemIndex).managedReferenceValue = obj;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif