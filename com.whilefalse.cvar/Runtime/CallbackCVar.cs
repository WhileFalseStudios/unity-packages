using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhileFalse.Cvar
{    
    /// <summary>
    /// A CVar that invokes its <see cref="callback"/> delegate when the value is changed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CallbackCVar<T> : CVar<T> where T : IConvertible
    {
        public delegate void Callback(T value);

        public Callback callback { get; set; }

        public CallbackCVar(string name, T defaultValue, string helpString = "", ConFlag flags = ConFlag.None) : base(name, defaultValue, helpString, flags)
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
