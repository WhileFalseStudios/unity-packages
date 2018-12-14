using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhileFalse.Cvar
{
    public abstract class ConBase
    {
        public string name { get; }
        public string helpString { get; }
        public ConFlag flags { get; }
        public virtual int parameterCount { get; }

        protected ConBase(string name, string helpString, ConFlag flags = ConFlag.None)
        {
            this.name = name;
            this.helpString = helpString;
            this.flags = flags;

            ConManager.Register(this);
        }

        /// <summary>
        /// Invoke this console object.
        /// </summary>
        /// <param name="args">An array of arguments that the object can use how it wishes.</param>
        public abstract void Call(params string[] args);

        /// <summary>
        /// Checks if the provided <see cref="ConFlag"/> mask matches this object's.
        /// </summary>
        /// <param name="mask">The mask to match</param>
        /// <returns>True if it matches, otherwise false.</returns>
        public bool HasFlag(ConFlag mask)
        {
            return flags.HasFlag(mask);
        }

        /// <summary>
        /// Converts the string to the specified type
        /// </summary>
        /// <typeparam name="T">The type to parse to</typeparam>
        /// <param name="input">The input string to parse</param>
        /// <param name="defaultValue">A default value to return if conversion fails.</param>
        /// <returns>The parsed value, or defaultValue if conversion fails.</returns>
        public virtual T ParseValue<T>(string input, T defaultValue) where T : IConvertible
        {
            try
            {
                return (T)Convert.ChangeType(input, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public abstract string GetTypeString();

        public abstract string GetConfigString();
    }
}
