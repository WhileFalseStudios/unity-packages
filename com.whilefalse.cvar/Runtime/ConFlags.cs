using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhileFalse.Cvar
{
    /// <summary>
    /// Flags used by the console system for controlling functionality.
    /// </summary>
    [System.Flags]
    public enum ConFlag
    {
        /// <summary>
        /// No flags in use
        /// </summary>
        None = 0,
        /// <summary>
        /// Can only be used when the game is running in cheat mode
        /// </summary>
        Cheat = 1 << 0,
        /// <summary>
        /// Value will be saved to config file on modify
        /// </summary>
        Archive = 1 << 1,
        /// <summary>
        /// Can only be used in debug mode (in editor by default)
        /// </summary>
        Debug = 1 << 2,
    }
}