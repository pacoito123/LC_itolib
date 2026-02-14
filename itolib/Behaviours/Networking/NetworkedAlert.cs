using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     Represents an alert entry to display to the player.
    /// </summary>
    public interface IAlertEntry
    {
        /// <summary>
        ///     Whether this alert entry should only be displayed once or not.
        /// </summary>
        bool SingleUse { get; set; }
    }

    /// <summary>
    ///     Represents an abstract alert or message to be sent to the player.
    /// </summary>
    /// <typeparam name="T">Any struct that implements <c>IAlertEntry</c> representing an alert to send to players.</typeparam>
    public abstract class NetworkedAlert<T> : NetworkBehaviour where T : struct, IAlertEntry
    {
        /// <summary>
        ///     Cached array for playing a single alert.
        /// </summary>
        private static readonly T[] alertSingle = new T[1];

        /// <summary>
        ///     List of alert entries to display to players.
        /// </summary>
        [Header("Networked Alert")]
        [Tooltip("List of alert entries to display to players.")]
        [FormerlySerializedAs("dialogueEntries")]
        [SerializeField] protected T[]? alertEntries;

        /// <summary>
        ///     <c>BitArray</c> containing whether the alert entry at each index has been exhausted or not.
        /// </summary>
        private BitArray? exhaustedAlerts;

        /// <summary>
        ///     Number of alerts that have been exhausted.
        /// </summary>
        private int exhaustedCount;

        /// <summary>
        ///     Initialize <c>BitArray</c> based on number of alert entires.
        /// </summary>
        protected virtual void Start()
        {
            if (alertEntries?.Length > 0)
            {
                exhaustedAlerts = new(alertEntries.Length);
            }
        }

        /// <summary>
        ///     Check if the alert entry should be skipped or not, and also set it to be.
        /// </summary>
        /// <param name="entry">Alert entry to check.</param>
        /// <param name="index">Index of the alert entry to check.</param>
        /// <returns>Whether the alert entry should be skipped or not.</returns>
        protected bool CheckSingleUse(IAlertEntry entry, int index)
        {
            // Don't skip if alert entry can be displayed more than once.
            if (!entry.SingleUse)
            {
                return false;
            }

            // Skip if alert entry is already exhausted.
            if (exhaustedAlerts == null || index >= exhaustedAlerts.Length || exhaustedAlerts[index])
            {
                return true;
            }

            // Set alert entry as exhausted.
            exhaustedAlerts[index] = true;
            exhaustedCount++;

            // Don't skip alert entry.
            return false;
        }

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
            // Check if all alerts have been exhausted.
            if (exhaustedCount == exhaustedAlerts?.Length)
            {
                return;
            }

            // Check if given index is valid.
            if (startingIndex < alertEntries?.Length)
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
            // Check if all alerts have been exhausted.
            if (exhaustedCount == exhaustedAlerts?.Length)
            {
                return;
            }

            // Check if given index is valid.
            if (startingIndex < alertEntries?.Length)
            {
                PlayAlerts(alertEntries, startingIndex);
            }
        }

        /// <summary>
        ///     Play a single alert entry.
        /// </summary>
        /// <param name="alertIndex">Index of the alert entry to play.</param>
        public void PlayAlertSingle(int alertIndex)
        {
            // Check if alert has been exhausted.
            if (exhaustedAlerts?[alertIndex] == true)
            {
                return;
            }

            // Check if given index is valid.
            if (alertIndex < alertEntries?.Length)
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
            // Check if alert has been exhausted.
            if (exhaustedAlerts?[alertIndex] == true)
            {
                return;
            }

            // Check if given index is valid.
            if (alertIndex < alertEntries?.Length)
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
        /// <param name="startingIndex">Index to skip to when playing the alert entries.</param>
        protected abstract void PlayAlerts(T[] alerts, int startingIndex = 0);
    }
}