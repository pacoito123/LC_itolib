using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayerHinderer : PlayerAttachable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Player Hinderer")]
        [Tooltip("")]
        public float hinderedMultiplier = 2.5f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Jumping")]
        [Tooltip("")]
        public bool allowJumping = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool requireStamina = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Quicksand")]
        [Tooltip("")]
        public bool sinkPlayer = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float sinkingSpeedMultiplier = 0.21f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Water")]
        [Tooltip("")]
        public bool drownPlayer = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool waterOverlay = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent<PlayerControllerB>? onHinderStart;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB>? onHinderStop;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public Collider? hindererCollider;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            AttachCondition = player => !player.isPlayerDead;
            DetachCondition = player => player.isPlayerDead;

            if (!TryGetComponent(out hindererCollider))
            {
                // Plugin.StaticLogger.LogWarning(""); // TODO: Warn collider is missing.
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Update()
        {
            if (LocalPlayerAttached && AttachedPlayer != null)
            {
                if (allowJumping && requireStamina && AttachedPlayer.isExhausted)
                {
                    AttachedPlayer.isExhausted = false;
                }

                /* if (drownPlayer)
                {
                    if (hindererCollider?.bounds.Contains(AttachedPlayer.gameplayCamera.transform.position) == true)
                    {
                        
                    }
                } */
            }

            base.Update();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            if (player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                return;
            }

            base.AttachPlayerLocal(player);

            player.isMovementHindered++;
            player.hinderedMultiplier *= hinderedMultiplier;

            if (sinkPlayer)
            {
                player.sourcesCausingSinking++;
                player.sinkingSpeedMultiplier = sinkingSpeedMultiplier;
            }
            else if (drownPlayer || allowJumping)
            {
                player.isUnderwater = true;

                if (drownPlayer)
                {
                    player.underwaterCollider = hindererCollider;
                }
            }

            onHinderStart?.Invoke(player);
            HinderPlayerServerRpc(player.GetComponent<NetworkObject>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (AttachedPlayer == null || (!attachLocally && AttachedPlayer.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId))
            {
                return;
            }

            AttachedPlayer.isMovementHindered--;
            AttachedPlayer.hinderedMultiplier /= hinderedMultiplier;

            if (sinkPlayer)
            {
                AttachedPlayer.sourcesCausingSinking--;
                AttachedPlayer.sinkingSpeedMultiplier = 0.0f;
            }
            else if (drownPlayer || allowJumping)
            {
                AttachedPlayer.isUnderwater = false;

                if (drownPlayer)
                {
                    AttachedPlayer.underwaterCollider = null;
                }
            }

            onHinderStop?.Invoke(AttachedPlayer);
            HinderPlayerServerRpc(AttachedPlayer.GetComponent<NetworkObject>(), stop: true);

            base.DetachPlayerLocal();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="stop"></param>
        [ServerRpc(RequireOwnership = false)]
        public void HinderPlayerServerRpc(NetworkObjectReference playerReference, bool stop = false)
        {
            HinderPlayerClientRpc(playerReference, stop);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="stop"></param>
        [ClientRpc]
        public void HinderPlayerClientRpc(NetworkObjectReference playerReference, bool stop = false)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                if (!stop)
                {
                    onHinderStart?.Invoke(player);
                }
                else
                {
                    onHinderStop?.Invoke(player);
                }
            }
        }
    }
}