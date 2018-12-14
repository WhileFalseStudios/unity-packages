using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhileFalse.Cvar
{
    /// <summary>
    /// An enum CVar that invokes its <see cref="callback"/> delegate when the value is changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CallbackCVarEnum<T> : CvarEnum<T> where T : struct, IConvertible, IFormattable, IComparable
    {
        public delegate void Callback(T value);

        public Callback callback { get; set; }

        public CallbackCVarEnum(string name, T defaultValue, string helpString = "", ConFlag flags = ConFlag.None) : base(name, defaultValue, helpString, flags)
        { }

        public override void Call(params string[] args)
        {
            base.Call(args);
            if (callback != null)
            {
                callback.Invoke(value);
            }
        }
    }
}