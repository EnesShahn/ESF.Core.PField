#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace ESF.Core.PField.Editor
{
    public static class ReflectionExtensions
    {
        public static Type[] GetChildTypes(this Type type, bool includeSelf = true)
        {
            List<Type> typesList = new List<Type>();
            if (includeSelf) typesList.Add(type);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (t != type && type.IsAssignableFrom(t))
                        typesList.Add(t);
                }
            }

            return typesList.ToArray();
        }

        public static Type GetGenericTypeDefinitionIfGeneric(this Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }
    }
}
#endif