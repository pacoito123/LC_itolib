using UnityEngine;

namespace itolib.Enums
{
    /// <summary>
    ///     Types of messages displayed to players.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        ///     Tip alert message (yellow).
        /// </summary>
        [Tooltip("Tip alert message (yellow).")]
        Tip,
        /// <summary>
        ///     Warning message (red).
        /// </summary>
        [Tooltip("Warning message (red).")]
        Warning,
        /// <summary>
        ///     Notification message (blue).
        /// </summary>
        [Tooltip("Notification message (blue).")]
        Notification
    }
}