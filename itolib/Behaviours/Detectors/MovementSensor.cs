using GameNetcodeStuff;
using itolib.Behaviours.Effects;
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
        ///     Attach if the player is alive.
        ///     Detach if the player is dead, or presses the set (<c>playerAction</c>).
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead;
            detachCondition = player => player.isPlayerDead;
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected override void Start()
        {
            // Try obtain player action that triggers this sensor.
            if (!GameNetworkManager.Instance.localPlayerController.TryFindMovementAction(out playerAction, actionToTrigger))
            {
                Plugin.StaticLogger.LogWarning($"Could not find movement action '{actionToTrigger}' defined for MovementSensor component in '{name}'!");
            }

            base.Start();
        }

        /// <summary>
        ///     Attempt to find player action every time the script is enabled (if missing).
        /// </summary>
        private void OnEnable()
        {
            // Try obtain player action that triggers this sensor, if it happens to be missing.
            if (playerAction == null && !GameNetworkManager.Instance.localPlayerController.TryFindMovementAction(out playerAction, actionToTrigger))
            {
                Plugin.StaticLogger.LogWarning($"Could not find movement action '{actionToTrigger}' defined for MovementSensor component in '{name}'!");

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

                // Check if the player action to detect was not pressed, or stopped being held.
                if (playerAction == null || (holdAction && !playerAction.IsPressed()) || (!holdAction && !playerAction.WasPerformedThisFrame()))
                {
                    return;
                }

                // Check if the player does not have enough stamina to trigger detection, if set to require stamina.
                if (actionRequiresStamina && attachedPlayer.isExhausted)
                {
                    return;
                }

                // Invoke movement detected event on the local client.
                onMovementDetected.Invoke(attachedPlayer);

                if (IsSpawned) // TODO: Separate local effect field?
                {
                    // Send movement detection to all other clients.
                    PlayerMovedRpc(attachedPlayer);
                }

                // Reset cooldown timer after triggering.
                timer = 0.0f;
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
                // Invoke movement detected event on all clients.
                onMovementDetected.Invoke(player);
            }
        }
    }
}