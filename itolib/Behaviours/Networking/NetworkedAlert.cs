using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     Represents an abstract alert or message to be sent to the player.
    /// </summary>
    /// <typeparam name="T">Any struct composing an alert to send to players.</typeparam>
    public abstract class NetworkedAlert<T> : NetworkBehaviour where T : struct
    {
        /// <summary>
        ///     List of alert entries to display to players.
        /// </summary>
        [Header("Networked Alert")]
        [Tooltip("List of alert entries to display to players.")]
        [FormerlySerializedAs("dialogueEntries")]
        [SerializeField] protected T[]? alertEntries;

        /// <summary>
        ///     Cached array for playing a single alert.
        /// </summary>
        private readonly T[] alertSingle = new T[1];

        /// <summary>
        ///     Play each alert entry sequentially, starting from the beginning.
        /// </summary>
        public void PlayAlertSequence()
        {
            PlayAlertSequence(0);
        }

        /// <summary>
        ///     Play each alert entry sequentially, starting from a specific point.
        /// </summary>
        /// <param name="startingIndex">Index to skip to when playing the alert entries.</param>
        public void PlayAlertSequence(int startingIndex)
        {
            // Check if given index is valid.
            if (alertEntries?.Length > startingIndex)
            {
                // Play alert sequence for the local client.
                PlayAlertSequenceLocal(startingIndex);

                // Check if object is spawned.
                if (IsSpawned)
                {
                    // Play alert sequence for all other clients.
                    PlayAlertSequenceRpc(startingIndex);
                }
            }
        }

        /// <summary>
        ///     Play alert sequence for all other clients.
        /// </summary>
        /// <param name="startingIndex">Index to skip to when playing the alert entries.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlayAlertSequenceRpc(int startingIndex)
        {
            // Display alert sequence for the local client.
            PlayAlertSequenceLocal(startingIndex);
        }

        /// <summary>
        ///     Play each alert entry sequentially for the local client, starting from the beginning.
        /// </summary>
        public void PlayAlertSequenceLocal()
        {
            PlayAlertSequenceLocal(0);
        }

        /// <summary>
        ///     Play each alert entry sequentially for the local client, starting from a specific point.
        /// </summary>
        /// <param name="startingIndex">Index to skip to when playing the alert entries.</param>
        public void PlayAlertSequenceLocal(int startingIndex)
        {
            // Check if given index is valid.
            if (alertEntries?.Length > startingIndex)
            {
                PlayAlerts(alertEntries[startingIndex..]);
            }
        }

        /// <summary>
        ///     Play a single alert entry.
        /// </summary>
        /// <param name="alertIndex">Index of the alert entry to play.</param>
        public void PlayAlertSingle(int alertIndex)
        {
            // Check if given index is valid.
            if (alertEntries?.Length > alertIndex)
            {
                // Play a single alert entry for the local client.
                PlayAlertSingleLocal(alertIndex);

                if (IsSpawned)
                {
                    // Play a single alert entry for all other clients.
                    PlayAlertSingleRpc(alertIndex);
                }
            }
        }

        /// <summary>
        ///     Play a single alert entry for all other clients.
        /// </summary>
        /// <param name="alertIndex">Index of the alert entry to play.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlayAlertSingleRpc(int alertIndex)
        {
            // Play a single alert entry for the local client.
            PlayAlertSingleLocal(alertIndex);
        }

        /// <summary>
        ///     Play a single alert entry for the local client.
        /// </summary>
        /// <param name="alertIndex">Index of the alert entry to play.</param>
        public void PlayAlertSingleLocal(int alertIndex)
        {
            // Check if given index is valid.
            if (alertEntries?.Length > alertIndex)
            {
                PlayAlert(alertEntries[alertIndex]);
            }
        }

        /// <summary>
        ///     Display a single alert entry.
        /// </summary>
        /// <param name="alert">Alert entry to display.</param>
        private void PlayAlert(T alert)
        {
            alertSingle[0] = alert;
            PlayAlerts(alertSingle);
        }

        /// <summary>
        ///     Display multiple alert entries, sequentially.
        /// </summary>
        /// <param name="alerts">Alert entries to display.</param>
        protected abstract void PlayAlerts(T[] alerts);
    }
}