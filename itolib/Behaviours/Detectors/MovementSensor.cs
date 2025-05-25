using GameNetcodeStuff;
using itolib.Behaviours.Effects;
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
        public InputAction? ActionToTrigger { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Movement Sensor")]
        [Tooltip("")]
        public float triggerInterval = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string actionToTrigger = "Move";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool holdAction = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent<PlayerControllerB>? onMovementDetected;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public float timer = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            AttachCondition = player => !player.isPlayerDead;
            DetachCondition = player => player.isPlayerDead;

            if (actionToTrigger.Length > 0)
            {
                // Get action (or key) that triggers this sensor.
                ActionToTrigger = GameNetworkManager.Instance.localPlayerController.playerActions.m_Movement.FindAction(actionToTrigger);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
        {
            if (ActionToTrigger == null)
            {
                enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (!LocalPlayerAttached || AttachedPlayer == null)
            {
                timer = triggerInterval;
                return;
            }

            if (timer < triggerInterval)
            {
                timer += Time.deltaTime;
                return;
            }

            if ((holdAction && !ActionToTrigger!.IsPressed()) || (!holdAction && !ActionToTrigger!.WasPerformedThisFrame()))
            {
                return;
            }

            onMovementDetected?.Invoke(AttachedPlayer);

            if (!isLocalEffect && IsSpawned)
            {
                PlayerMovedServerRpc(AttachedPlayer.GetComponent<NetworkObject>());
            }

            timer = 0.0f;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        public void PlayerMovedServerRpc(NetworkObjectReference playerReference)
        {
            PlayerMovedClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void PlayerMovedClientRpc(NetworkObjectReference playerReference)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                onMovementDetected?.Invoke(player);
            }
        }
    }
}