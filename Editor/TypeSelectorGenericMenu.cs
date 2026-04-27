#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ESF.Core.PField.Editor
{
    public class TypeSelectorGenericMenu
    {
        private GenericMenu genericMenu;
        private Type[] types;
        private Action<Type> OnTypeSelected;

        public TypeSelectorGenericMenu(Type[] types, Action<Type> onTypeSelectedCallback)
        {
            this.types = types;
            OnTypeSelected = onTypeSelectedCallback;
            InitGenericMenu();
        }

        private void InitGenericMenu()
        {
            genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("<Null>"), false, () => { OnTypeSelected?.Invoke(null); });

            for (int i = 0; i < types.Length; i++)
            {
                var typeName = types[i].Name;
                var assemblyName = types[i].Assembly.GetName().Name;
                var type = types[i];
                GUIContent content = new GUIContent($"{typeName} ({assemblyName})");
                genericMenu.AddItem(content, false, () => { OnTypeSelected?.Invoke(type); });
            }
        }
        public void ShowAsContext()
        {
            genericMenu.ShowAsContext();
        }
    }
}
#endif