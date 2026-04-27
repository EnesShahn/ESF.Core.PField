#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;

namespace ESF.Core.PField.Editor
{
    public static class SerializedPropertyExtensions
    {
        private static string pattern = @".*\[(\d*)\]";
        private static Regex rx = new Regex(pattern);

        public static object GetObjectValue(this SerializedProperty property)
        {
            var iterator = GetSerializedPropertyEnumerator(property);
            object currentObject = null;
            while (iterator.MoveNext())
            {
                currentObject = iterator.Current.Value;
            }

            return currentObject;
        }
        public static T GetObjectValue<T>(this SerializedProperty property)
        {
            return (T)GetObjectValue(property);
        }
        public static Type GetPropertyFieldType(this SerializedProperty property)
        {
            var iterator = GetSerializedPropertyEnumerator(property);
            while (iterator.MoveNext())
            {
                if (iterator.Current.Property.propertyPath == property.propertyPath)
                    return iterator.Current.FieldType;
            }

            return null;
        }

        public static bool IsAnyAncestorOfType(this SerializedProperty property, params Type[] checkTypes)
        {
            var propValue = property.GetObjectValue();
            var iterator = GetSerializedPropertyEnumerator(property);
            while (iterator.MoveNext())
            {
                if (iterator.Current.Value.Equals(propValue)) break;
                var currentObjectTypeCheck = iterator.Current.Value.GetType().GetGenericTypeDefinitionIfGeneric();
                foreach (var checkType in checkTypes)
                {
                    if (currentObjectTypeCheck == checkType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public static bool IsAnyAncestorArray(this SerializedProperty property)
        {
            var propValue = property.GetObjectValue();
            var iterator = GetSerializedPropertyEnumerator(property);
            bool isArray = false;
            while (iterator.MoveNext())
            {
                if (iterator.Current.Value.Equals(propValue)) break;
                var currentObjectTypeCheck = iterator.Current.Value.GetType().GetGenericTypeDefinitionIfGeneric();
                if (currentObjectTypeCheck.IsArray)
                {
                    isArray = true;
                }
            }

            return isArray;
        }
        public static bool IsAnyArrayAncestorNotOfType(this SerializedProperty property, Type checkType)
        {
            var iterator = GetSerializedPropertyEnumerator(property);
            while (iterator.MoveNext())
            {
                var currentObjectType = iterator.Current.Value.GetType().GetGenericTypeDefinitionIfGeneric();

                if (currentObjectType == checkType)
                {
                    iterator.MoveNext();
                    continue;
                }

                if (currentObjectType == typeof(List<>) || currentObjectType.IsArray)
                    return true;
            }

            return false;
        }
        public static SerializedProperty GetParentSerializedProperty(this SerializedProperty property)
        {
            var propValue = property.GetObjectValue();
            var iterator = GetSerializedPropertyEnumerator(property);
            SerializedProperty parentSP = null;
            while (iterator.MoveNext())
            {
                if (iterator.Current.Value.Equals(propValue)) break;
                parentSP = iterator.Current.Property;
            }

            return parentSP;
        }
        public static IEnumerator<SPInfo> GetSerializedPropertyEnumerator(this SerializedProperty property)
        {
            property = property.Copy();
            SerializedObject serializedObject = property.serializedObject;
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            string pathAdjusted = property.propertyPath.Replace(".Array.data", ".ARR");
            string[] propPathSplit = pathAdjusted.Split('.');

            Type containerObjectType = serializedObject.targetObject.GetType();
            FieldInfo currentObjectFieldInfo = containerObjectType.GetField(propPathSplit[0], bindingFlags);
            object currentObject = currentObjectFieldInfo.GetValue(serializedObject.targetObject);
            SerializedProperty currentProperty = serializedObject.FindProperty(propPathSplit[0]);

            yield return new SPInfo(currentProperty.Copy(), currentObject, currentObjectFieldInfo.FieldType);
            for (int i = 1; i < propPathSplit.Length; i++)
            {
                var currentObjectType = currentObject.GetType();

                Type fieldType = null;

                if (propPathSplit[i].StartsWith("ARR["))
                {
                    var pathRegexMatch = rx.Match(propPathSplit[i]);
                    int index = Convert.ToInt32(pathRegexMatch.Groups[1].Value);

                    Array array = null;
                    if (currentObjectType.IsGenericType && currentObjectType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        currentObjectFieldInfo = currentObjectType.GetField("_items", bindingFlags);
                        fieldType = currentObjectFieldInfo.FieldType;
                        array = (Array)currentObjectFieldInfo.GetValue(currentObject);
                    }
                    else if (currentObjectType.IsArray)
                    {
                        fieldType = currentObject.GetType();
                        array = (Array)currentObject;
                    }

                    if (index <= array.Length) // Happens when we are trying to get the item at index before it being applied to the SerializedObject (Only occurs on nested arrays)
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        //UnityEngine.Debug.LogWarning($"The item at index {index} is not yet applied to the object. forced ApplyModifiedProperties() on SerializedObject");
                    }

                    currentProperty = currentProperty.GetArrayElementAtIndex(index);
                    currentObject = array.GetValue(index);
                }
                else
                {
                    currentProperty = currentProperty.FindPropertyRelative(propPathSplit[i]);
                    currentObjectFieldInfo = currentObjectType.GetField(propPathSplit[i], bindingFlags);
                    fieldType = currentObjectFieldInfo.FieldType;
                    currentObject = currentObjectFieldInfo.GetValue(currentObject);
                }

                yield return new SPInfo(currentProperty.Copy(), currentObject, fieldType);
            }

            yield break;
        }

        public static bool HasAttribute(this SerializedProperty property, Type attributeType)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var members = property.serializedObject.targetObject.GetType().GetMember(property.propertyPath, flags);

            foreach (var member in members)
            {
                if (member.CustomAttributes.Count() == 0) continue;
                var attribute = member.GetCustomAttribute(attributeType);
                if (attribute == null) continue;
                if (member.Name != property.propertyPath) continue;
                return true;
            }

            return false;
        }
        public static Attribute GetAttribute(this SerializedProperty property, Type attributeType)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var members = property.serializedObject.targetObject.GetType().GetMember(property.propertyPath, flags);

            foreach (var member in members)
            {
                if (member.CustomAttributes.Count() == 0) continue;
                var attribute = member.GetCustomAttribute(attributeType);
                if (attribute == null) continue;
                if (member.Name != property.propertyPath) continue;
                return attribute;
            }

            return null;
        }

        public static void SetExpandRecursively(this SerializedProperty property, bool isExpanded)
        {
            SerializedProperty propCopy = property.Copy();
            propCopy.isExpanded = isExpanded;

            int depth = propCopy.depth;
            while (propCopy.NextVisible(true) && propCopy.depth > depth)
            {
                if (propCopy.hasVisibleChildren)
                {
                    propCopy.isExpanded = isExpanded;
                }
            }
        }
    }

    public class SPInfo
    {
        public SerializedProperty Property;
        public object Value;
        public Type FieldType;

        public SPInfo(SerializedProperty property, object value, Type type)
        {
            Property = property;
            Value = value;
            FieldType = type;
        }
    }
}
#endif