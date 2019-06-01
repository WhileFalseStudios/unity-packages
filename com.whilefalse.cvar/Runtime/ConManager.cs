using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhileFalse.Cvar
{
    public static class ConManager
    {
        private static bool _cheatsActive;
        public static bool cheatsActive
        {
            get => _cheatsActive | debugActive;
            set => _cheatsActive = value;
        }

        private static bool _debugActive;
        public static bool debugActive
        {
#if UNITY_EDITOR
            get => true;
#else
            get => _debugActive;
#endif
            set => _debugActive = value;
        }

        private static Dictionary<string, ConBase> consoleItemMap = new Dictionary<string, ConBase>();

        internal static void Register(ConBase obj)
        {
            if (consoleItemMap.ContainsKey(obj.name))
            {
                Debug.LogWarningFormat("Multiple console objects have the name {0}. This one will not be registered.", obj.name);
                return;
            }

            consoleItemMap.Add(obj.name, obj);
        }

        /// <summary>
        /// Finds a <typeparamref name="T"/> given its name.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ConBase"/> to find.</typeparam>
        /// <param name="name">The name of the <typeparamref name="T"/> to find.</param>
        /// <returns>The found <typeparamref name="T"/> if found and of the right type, otherwise <see cref="null"/>.</returns>
        public static T Find<T>(string name) where T : ConBase
        {
            if (consoleItemMap.ContainsKey(name))
            {
                return consoleItemMap[name] as T;
            }
            else return default;
        }

        public static List<ConBase> Search(string key)
        {
            if (string.IsNullOrEmpty(key))
                return consoleItemMap.Values.ToList();
            
            List<ConBase> items = new List<ConBase>();
            foreach (var c in consoleItemMap)
            {
                if (c.Key.Contains(key))
                {
                    items.Add(c.Value);
                }
            }

            return items;
        }
    }
}
