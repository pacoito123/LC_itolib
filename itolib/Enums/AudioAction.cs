using System;
using UnityEngine;

namespace itolib.Enums
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Obsolete("AudioGroup component that uses this is deprecated.")]
    public enum AudioAction
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        Initialize = -1,
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        Play,
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        Pause,
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        Unpause,
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        Stop,
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        StopIncludingOneShots,
    }
}