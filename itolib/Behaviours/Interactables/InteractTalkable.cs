using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine.Events;

namespace itolib.Behaviours.Interactables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class InteractTalkable : InteractTrigger
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public bool IsActive { get; private set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent<PlayerControllerB>? onStartTalking;

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent<PlayerControllerB>? onStopTalking;

        /// <summary>
        ///     Set default talkable properties.
        /// </summary>
        private void Reset()
        {
            hoverTip = "Transmit voice : [LMB]";
            interactable = true;
            oneHandedItemAllowed = false;
            twoHandedItemAllowed = false;

            holdInteraction = true;
            timeToHold = 1.0f;
            timeToHoldSpeedMultiplier = 0.0f;
            holdTip = "Transmitting voice...";

            cooldownTime = 0.5f;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            holdingInteractEvent.AddListener(EnableWalkieLocal);
            onStopInteract.AddListener(DisableWalkieLocal);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="_"></param>
        public void EnableWalkieLocal(float _)
        {
            if (IsActive)
            {
                return;
            }
            IsActive = true;

            EnableWalkieServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        public void EnableWalkieServerRpc(NetworkObjectReference playerReference)
        {
            EnableWalkieClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void EnableWalkieClientRpc(NetworkObjectReference playerReference)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player))
            {
                player.holdingWalkieTalkie = true;
                player.speakingToWalkieTalkie = true;
                StartOfRound.Instance.UpdatePlayerVoiceEffects();

                onStartTalking?.Invoke(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="_"></param>
        public void DisableWalkieLocal(PlayerControllerB _)
        {
            DisableWalkieServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>());
            IsActive = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        public void DisableWalkieServerRpc(NetworkObjectReference playerReference)
        {
            DisableWalkieClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void DisableWalkieClientRpc(NetworkObjectReference playerReference)
        {
            if (playerReference.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player))
            {
                onStopTalking?.Invoke(player);

                player.holdingWalkieTalkie = false;
                player.speakingToWalkieTalkie = false;
                StartOfRound.Instance.UpdatePlayerVoiceEffects();
            }
        }
    }
}