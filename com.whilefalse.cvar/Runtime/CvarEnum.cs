using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhileFalse.Cvar
{
    internal interface IEnumVariable
    {
        List<string> GetEnumValues();
    }

    /// <summary>
    /// CVar specifically for enums. Allows the enum's name to be used in place of its integer value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CvarEnum<T> : CVar<T>, IEnumVariable where T : struct, IConvertible, IFormattable, IComparable
    {
        private bool m_invalid;

        public CvarEnum(string name, T defaultValue, string helpString = "", ConFlag flags = ConFlag.None) : base(name, defaultValue, helpString, flags)
        {
            if (!typeof(T).IsEnum)
            {
                Debug.LogErrorFormat("CVarEnum<{0}> is not a valid type because {0} is not defined as an Enum.", typeof(T).FullName);
                m_invalid = true;
            }
        }

        public override T2 ParseValue<T2>(string input, T2 defaultValue)
        {
            if (m_invalid)
                return defaultValue;

            try
            {
                return (T2)Enum.Parse(typeof(T2), input);
            }
            catch
            {
                return defaultValue;
            }
        }            

        public override T value
        {
            get => m_cachedValue;
            set
            {
                if (m_invalid)
                    return;

                var range = Enum.GetValues(typeof(T)).Cast<T>();
                var max = range.Max();
                var min = range.Min();

                if (value.CompareTo(max) > 0)
                    m_cachedValue = max;
                if (value.CompareTo(min) < 0)
                    m_cachedValue = min;
                else
                    m_cachedValue = value;
            }
        }

        /// <summary>
        /// Gets all the possible values this enum can have.
        /// </summary>
        /// <returns></returns>
        public List<string> GetEnumValues()
        {
            return Enum.GetNames(typeof(T)).ToList();
        }
    }
}
