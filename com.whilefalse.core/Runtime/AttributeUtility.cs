using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WhileFalse.Core
{
    public static class AttributeUtility
    {
        public static IEnumerable<Tuple<Type, T>> GetTypesWithAttribute<T>() where T : Attribute
        {
            foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
            {
                var attr = type.GetCustomAttribute<T>(true);
                if (attr != null)
                {
                    yield return new Tuple<Type, T>(type, attr);
                }
            }
        }
    }
}
