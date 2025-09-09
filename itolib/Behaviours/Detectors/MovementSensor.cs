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
    ///     TODO.
    /// </summary>
    public class MovementSensor : PlayerAttachable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Movement Sensor")]
        [Tooltip("")]
        [SerializeField] private float triggerInterval = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected string actionToTrigger = "Move";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] protected bool holdAction = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool actionRequiresStamina;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] protected UnityEvent<PlayerControllerB> onMovementDetected = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private float timer;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected InputAction? playerAction;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead;
            detachCondition = player => player.isPlayerDead;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            if (actionToTrigger.Length > 0 && playerAction == null)
            {
                // Get action (or key) that triggers this sensor.
                playerAction = GameNetworkManager.Instance.localPlayerController.playerActions.m_Movement.FindAction(actionToTrigger, throwIfNotFound: true);
            }

            timer = triggerInterval;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (localPlayerAttached && attachedPlayer != null)
            {
                if (timer < triggerInterval)
                {
                    timer += Time.deltaTime;

                    return;
                }

                if ((holdAction && !playerAction!.IsPressed()) || (!holdAction && !playerAction!.WasPerformedThisFrame()))
                {
                    return;
                }

                if (actionRequiresStamina && attachedPlayer.isExhausted)
                {
                    return;
                }

                onMovementDetected.Invoke(attachedPlayer);

                if (IsSpawned) // TODO: Separate local effect field?
                {
                    PlayerMovedServerRpc(attachedPlayer);
                }

                timer = 0.0f;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        private void PlayerMovedServerRpc(NetworkBehaviourReference playerReference)
        {
            PlayerMovedClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        private void PlayerMovedClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                onMovementDetected.Invoke(player);
            }
        }
    }
}