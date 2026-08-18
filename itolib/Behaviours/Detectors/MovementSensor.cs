using GameNetcodeStuff;
using itolib.Behaviours.Networking;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     Detects a specific <c>PlayerAction</c> input continuously over a defined interval.
    /// </summary>
    public class MovementSensor : PlayerAttachable
    {
        /// <summary>
        ///     Cooldown interval between each movement detection, in seconds.
        /// </summary>
        [Header("Movement Sensor")]
        [Tooltip("Cooldown interval between each movement detection, in seconds.")]
        [Min(0.0f)]
        [SerializeField] private float triggerInterval = 1.0f;

        /// <summary>
        ///    <c>PlayerAction</c> to detect from the attached player. See <c>PlayerActions.MovementActions</c> for a list of valid player actions.
        /// </summary>
        [Tooltip("PlayerAction to detect from the attached player. See 'PlayerActions.MovementActions' for a list of valid player actions.")]
        [SerializeField] protected string actionToTrigger = "Move";

        /// <summary>
        ///     Whether the specified player action can be held to trigger continuously or not.
        /// </summary>
        [Tooltip("Whether the specified player action can be held to trigger continuously or not.")]
        [SerializeField] protected bool holdAction = true;

        /// <summary>
        ///     Whether the specified player action requires the player to have some stamina in order to trigger or not.
        /// </summary>
        [Tooltip("Whether the specified player action requires the player to have some stamina in order to trigger or not.")]
        [SerializeField] private bool actionRequiresStamina;

        /// <summary>
        ///     Callback invoked when movement is successfully detected, with the player in question as parameter.
        /// </summary>
        [Header("Events")]
        [Tooltip("Callback invoked when movement is successfully detected, with the player in question as parameter.")]
        [SerializeField] protected UnityEvent<PlayerControllerB> onMovementDetected = new();

        /// <summary>
        ///     Time passed since the last movement detection.
        /// </summary>
        private float timer;

        /// <summary>
        ///     Cached reference to the player action to detect.
        /// </summary>
        protected InputAction? playerAction;

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
            // Try obtain player action that triggers this sensor.
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null
                || !GameNetworkManager.Instance.localPlayerController.TryFindMovementAction(out playerAction, actionToTrigger))
            {
                Plugin.Logger.LogWarning($"Could not find movement action '{actionToTrigger}' defined for MovementSensor component in '{name}'!");
            }

            base.Start();
        }

        /// <summary>
        ///     Attempt to find player action every time the script is enabled (if missing).
        /// </summary>
        private void OnEnable()
        {
            // Try obtain player action that triggers this sensor, if it happens to be missing.
            if (playerAction == null && (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null
                || !GameNetworkManager.Instance.localPlayerController.TryFindMovementAction(out playerAction, actionToTrigger)))
            {
                Plugin.Logger.LogWarning($"Could not find movement action '{actionToTrigger}' defined for MovementSensor component in '{name}'!");

                // Disable script if the player action was not found.
                enabled = false;

                return;
            }

            // Set cooldown timer to detect movement immediately after being enabled. TODO: Maybe add field for this?
            timer = triggerInterval;
        }

        /// <summary>
        ///     Handle movement detection and its cooldown interval.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (localPlayerAttached && attachedPlayer != null)
            {
                // Check if detection is currently on cooldown.
                if (timer < triggerInterval)
                {
                    // Increment timer.
                    timer += Time.deltaTime;

                    return;
                }

                // Check if the player action stopped being held, if set as a hold action.
                if (!holdAction || playerAction == null || !playerAction.IsPressed())
                {
                    return;
                }

                // Check if the player does not have enough stamina to trigger detection, if set to require stamina.
                if (actionRequiresStamina && attachedPlayer.isExhausted)
                {
                    return;
                }

                // Trigger movement detection.
                PlayerMoved(attachedPlayer);
            }
        }

        /// <summary>
        ///     Attach player on the local client.
        /// </summary>
        /// <param name="player">Player to attach.</param>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            if (localPlayerAttached && !holdAction && playerAction != null)
            {
                // Subscribe to the action starting event, if not set as a hold action.
                playerAction.started += PlayerMoved;
            }
        }

        /// <summary>
        ///     Detach player on the local client.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (localPlayerAttached && !holdAction && playerAction != null)
            {
                // Unsubscribe to the action starting event, if not set as a hold action.
                playerAction.started -= PlayerMoved;
            }

            base.DetachPlayerLocal();
        }

        /// <summary>
        ///     Trigger movement detection from an attached player.
        /// </summary>
        private void PlayerMoved(InputAction.CallbackContext _)
        {
            if (localPlayerAttached && attachedPlayer != null)
            {
                // Check if detection is currently on cooldown.
                if (timer < triggerInterval)
                {
                    return;
                }

                // Check if the player does not have enough stamina to trigger detection, if set to require stamina.
                if (actionRequiresStamina && attachedPlayer.isExhausted)
                {
                    return;
                }

                // Trigger movement detection from the attached player.
                PlayerMoved(attachedPlayer);
            }
        }

        /// <summary>
        ///     Trigger movement detection from an attached player.
        /// </summary>
        /// <param name="player">Player whose movement was detected.</param>
        protected virtual void PlayerMoved(PlayerControllerB player)
        {
            // Trigger movement detection on the local client.
            PlayerMovedLocal(player);

            if (IsSpawned) // TODO: Separate local effect field?
            {
                // Send movement detection to all other clients.
                PlayerMovedRpc(player);
            }
        }

        /// <summary>
        ///     Trigger movement detection on all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the detected player.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlayerMovedRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Trigger movement detection on the local client.
                PlayerMovedLocal(player);
            }
        }

        /// <summary>
        ///     Trigger movement detection on the local client.
        /// </summary>
        /// <param name="player">Player whose movement was detected.</param>
        protected virtual void PlayerMovedLocal(PlayerControllerB player)
        {
            // Invoke movement detected callback, with the detected player as parameter.
            onMovementDetected.Invoke(player);

            if (localPlayerAttached || !attachLocally)
            {
                // Reset cooldown timer after triggering.
                timer = 0.0f;
            }
        }
    }
}