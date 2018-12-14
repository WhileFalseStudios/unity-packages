using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhileFalse.Cvar
{
    /// <summary>
    /// Global console variable of the specified type.
    /// <para>CVars should *always* be specified as static readonly variables of a class.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CVar<T> : ConBase where T : IConvertible
    {
        protected T m_cachedValue;

        /// <summary>
        /// The value of this cvar
        /// </summary>
        public virtual T value
        {
            get => m_cachedValue;
            set
            {
                if (!m_cachedValue.Equals(value))
                {
                    m_cachedValue = value;
                }
            }
        }

        public CVar(string name, T defaultValue, string helpString = "", ConFlag flags = ConFlag.None) : base(name, helpString, flags)
        {
            m_cachedValue = defaultValue;
        }

        /// <summary>
        /// Sets the value of this cvar.
        /// </summary>
        /// <param name="args"></param>
        public override void Call(params string[] args)
        {
            if (args.Length == 0)
                return;

            if (HasFlag(ConFlag.Debug) && !ConManager.debugActive)
                return;

            if (HasFlag(ConFlag.Cheat) && !ConManager.cheatsActive)
                return;

            value = ParseValue(args[0], value);
        }

        public override string GetConfigString()
        {
            return string.Empty;
        }

        public override string GetTypeString()
        {
            return typeof(T).Name;
        }
    }
}
