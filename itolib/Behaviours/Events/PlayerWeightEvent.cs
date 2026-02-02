using GameNetcodeStuff;
using itolib.Behaviours.Networking;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Events
{
    /// <summary>
    ///     Performs an event periodically, with trigger chances being dependant on the attached player's weight.
    /// </summary>
    public class PlayerWeightEvent : PlayerAttachable
    {
        /// <summary>
        ///     Interval between each weight check, in seconds.
        /// </summary>
        /// <remarks>Can be left at <c>0</c> to disable automatic weight checking.</remarks>
        [Space(5.0f)]
        [Header("Player Weight Event")]
        [Tooltip("Interval between each weight check, in seconds. Can be left at '0' to disable automatic weight checking.")]
        [Min(0.0f)]
        [SerializeField] private float weightInterval = 5.0f;

        /// <summary>
        ///     Base chance for the weight event to trigger, regardless of player weight.
        /// </summary>
        [Tooltip("Base chance for the weight event to trigger, regardless of player weight.")]
        [Range(0.0f, 100.0f)]
        [SerializeField] private float baseWeightChance;

        /// <summary>
        ///     Multiplier applied to the player's weight when calculating percentage chance to trigger.
        /// </summary>
        /// <remarks>Determines what percentage of player weight contributes to trigger chance.</remarks>
        [Tooltip("Multiplier applied to the player's weight when calculating percentage chance to trigger. Determines what percentage of player "
            + "weight contributes to trigger chance.")]
        [Range(0.0f, 5.0f)]
        [SerializeField] private float weightMultiplier = 0.5f;

        /// <summary>
        ///     Amount to increase the chance for the weight event to trigger every time a weight check fails.
        /// </summary>
        [Tooltip("Amount to increase the chance for the weight event to trigger every time a weight check fails.")]
        [Range(0.0f, 100.0f)]
        [SerializeField] private float chanceIncrease;

        /// <summary>
        ///     Whether the additional chance for the weight event to trigger resets when the event is triggered or not.
        /// </summary>
        [Tooltip("Whether the additional chance for the weight event to trigger resets when the event is triggered or not.")]
        [SerializeField] private bool resetIncreaseOnTrigger;

        /// <summary>
        ///     Whether the additional chance for the weight event to trigger resets when the player detaches or not.
        /// </summary>
        [Tooltip("Whether the additional chance for the weight event to trigger resets when the player detaches or not.")]
        [SerializeField] private bool resetIncreaseOnDetach;

        /// <summary>
        ///     Callback invoked when the weight event is successfully triggered after a weight check.
        /// </summary>
        [Space(5.0f)]
        [Header("Events")]
        [Tooltip("Callback invoked when the weight event is triggered after a weight check.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onCheckSuccess = new();

        /// <summary>
        ///     Callback invoked when the weight event fails to be triggered after a weight check.
        /// </summary>
        [Tooltip("Callback invoked when the weight event fails to be triggered after a weight check.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onCheckFail = new();

        /// <summary>
        ///     Time passed since the last weight check.
        /// </summary>
        private float timer;

        /// <summary>
        ///     Additional chance for the event to trigger on every weight check.
        /// </summary>
        private float addedChance;

        /// <summary>
        ///     Set some default values.
        /// </summary>
        protected override void Reset()
        {
            // Attaching locally is recommended for multiple players to be able to trigger the event.
            attachLocally = true;
            detachOnExit = true;
        }

        /// <summary>
        ///     Attach if the player is alive and enabled.
        /// </summary>
        /// <param name="player">Player to check for attaching.</param>
        /// <returns>Whether the player should attach or not.</returns>
        protected override bool AttachCondition(PlayerControllerB player)
        {
            return !player.isPlayerDead && player.isActiveAndEnabled;
        }

        /// <summary>
        ///     Detach if the player is dead or disabled.
        /// </summary>
        /// <param name="player">Player to check for detaching.</param>
        /// <returns>Whether the player should detach or not.</returns>
        protected override bool DetachCondition(PlayerControllerB player)
        {
            return player.isPlayerDead || !player.isActiveAndEnabled;
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected override void Start()
        {
            // Set initial trigger chance to base amount.
            addedChance = baseWeightChance;

            base.Start();
        }

        /// <summary>
        ///     Handle automatic weight checking for attached players.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (localPlayerAttached && attachedPlayer != null && weightInterval > 0.0f)
            {
                // Check if weight check is on cooldown.
                if (timer < weightInterval)
                {
                    // Increment timer.
                    timer += Time.deltaTime;

                    return;
                }

                // Perform weight check on the local client.
                PerformWeightCheck(attachedPlayer);

                // Reset cooldown timer after performing a check.
                timer = 0.0f;
            }
        }

        /// <summary>
        ///    Detach player on the local client.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (resetIncreaseOnDetach)
            {
                // Reset trigger chance upon detaching.
                addedChance = baseWeightChance;
            }

            base.DetachPlayerLocal();
        }

        /// <summary>
        ///     Perform a weight check for the local player, if attached.
        /// </summary>
        public void PerformWeightCheck()
        {
            if (localPlayerAttached && attachedPlayer != null)
            {
                PerformWeightCheck(attachedPlayer);
            }
        }

        /// <summary>
        ///     Perform a weight check for a specific player.
        /// </summary>
        /// <param name="player">Player whose weight should be checked.</param>
        public void PerformWeightCheck(PlayerControllerB player)
        {
            // Obtain displayed player weight, in pounds (lbs).
            float playerWeight = (player.carryWeight - 1) * 105.0f;

            // Perform roll for the weight check.
            bool trigger = Random.Range(0.0f, 100.0f) < (playerWeight * weightMultiplier) + addedChance;

            // Invoke weight event on the local client.
            InvokeEventLocal(player, !trigger);

            if (IsSpawned) // TODO: Separate local effect field?
            {
                // Send weight event to all other clients.
                InvokeEventRpc(player, !trigger);
            }
        }

        /// <summary>
        ///     Trigger weight event on all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the player whose weight was checked.</param>
        /// <param name="failed">Whether the weight check was successful or not.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void InvokeEventRpc(NetworkBehaviourReference playerReference, bool failed = false)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Invoke weight event on the local client.
                InvokeEventLocal(player, failed);
            }
        }

        /// <summary>
        ///     Trigger weight event on the local client.
        /// </summary>
        /// <param name="playerWhoTriggered">Player whose weight was checked.</param>
        /// <param name="failed">Whether the weight check was successful or not.</param>
        private void InvokeEventLocal(PlayerControllerB playerWhoTriggered, bool failed = false)
        {
            if (!failed)
            {
                // Invoke successful weight check event.
                onCheckSuccess.Invoke(playerWhoTriggered);

                if (resetIncreaseOnTrigger)
                {
                    // Reset trigger chance upon triggering.
                    addedChance = baseWeightChance;
                }
            }
            else
            {
                // Invoke failed weight check event.
                onCheckFail.Invoke(playerWhoTriggered);

                if (!resetIncreaseOnDetach || (attachLocally && playerWhoTriggered.IsLocalClient()))
                {
                    // Add trigger chance upon failed weight check.
                    addedChance += chanceIncrease;
                }
            }
        }
    }
}