using DunGen;
using itolib.Enums;
using LethalLevelLoader;

namespace itolib.Interfaces
{
    /// <summary>
    ///     Adds automatic activation capabilities to any implementing class.
    /// </summary>
    public interface IActivationScript : IDungeonCompleteReceiver
    {
        /// <summary>
        ///     Cached instance of the implementing script as an <c>IActivationScript</c>, to avoid having to cast. 
        /// </summary>
        IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the script.
        /// </summary>
        ActivationTime ActivationTime { get; set; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        bool PerformedActivation { get; set; }

        /// <summary>
        ///     Schedule activation for the script, or perform activation immediately depending on the set <c>ActivationTime</c>.
        /// </summary>
        void Initialize()
        {
            // Check if activation has already been performed.
            if (PerformedActivation)
            {
                return;
            }

            // Subscribe to the event corresponding to the set activation time, or activate immediately.
            switch (ActivationTime)
            {
                case ActivationTime.Immediate:
                    PerformActivation();
                    break;
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(PerformActivation);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(PerformActivation);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.AddListener(PerformActivation);
                    }
                    break;
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        void PerformActivation(ActivationTime activationTime);

        /// <summary>
        ///     Toggle activation status, unsubscribe to events, and perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        private void PerformActivation()
        {
            // Check if activation has already been performed, or the script is set to be manually activated.
            if (PerformedActivation || ActivationTime is ActivationTime.Manual)
            {
                return;
            }

            // Set activation as performed.
            PerformedActivation = true;

            // Unsubscribe from events.
            UnsubscribeFromEvents();

            // Perform script activation at the set activation time.
            PerformActivation(ActivationTime);
        }

        /// <summary>
        ///     Unsubscribe from any events that may have been subscribed to.
        /// </summary>
        void UnsubscribeFromEvents()
        {
            DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(PerformActivation);
            DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(PerformActivation);
            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.StartNewRoundEvent.RemoveListener(PerformActivation);
            }
        }

        /// <summary>
        ///     Trigger script activation on <c>Dungeon</c> generation completion.
        /// </summary>
        /// <remarks>Meant to be used alongside <c>IDungeonCompleteReceiver</c>'s <c>OnDungeonComplete()</c>.</remarks>
        void OnDungeonComplete()
        {
            if (ActivationTime is ActivationTime.DungeonComplete)
            {
                PerformActivation();
            }
        }
    }
}