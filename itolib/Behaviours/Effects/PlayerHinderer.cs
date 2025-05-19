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
        [Tooltip("")]
        public bool allowJumping = true;

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
        public void Awake()
        {
            AttachCondition = player => !player.isPlayerDead;
            DetachCondition = player => player.isPlayerDead;
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

            if (allowJumping)
            {
                player.isUnderwater = true;
            }

            onHinderStart?.Invoke(player);
            HinderPlayerServerRpc(player.GetComponent<NetworkObject>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (AttachedPlayer == null || (!isLocalEffect && AttachedPlayer.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId))
            {
                return;
            }

            AttachedPlayer.isMovementHindered--;
            AttachedPlayer.hinderedMultiplier /= hinderedMultiplier;

            if (allowJumping)
            {
                AttachedPlayer.isUnderwater = false;
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