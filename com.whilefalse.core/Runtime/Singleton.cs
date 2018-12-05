using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhileFalse.Core
{
    /// <summary>
    /// A class providing access to a single instance of a <see cref="Component"/> at runtime.
    /// </summary>
    /// <typeparam name="T">The component type to provide access to.</typeparam>
    public class Singleton<T> where T : Component
    {
        private static T _instance;

        /// <summary>
        /// <para>The instance of the singleton to provide.</para>
        /// <para>This is lazy initialized - it is created on first access of the property.</para>
        /// </summary>
        public static T instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                GameObject instanceContainer = new GameObject(string.Format("[{0} Singleton]", typeof(T).Name), typeof(T));
                _instance = instanceContainer.GetComponent<T>();
                return _instance;
            }
        }
    }
}
