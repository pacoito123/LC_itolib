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
        public bool IsActive { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent<PlayerControllerB> onStartTalking = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public UnityEvent<PlayerControllerB> onStopTalking = new();

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

            EnableWalkieServerRpc(GameNetworkManager.Instance.localPlayerController);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        public void EnableWalkieServerRpc(NetworkBehaviourReference playerReference)
        {
            EnableWalkieClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void EnableWalkieClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                player.holdingWalkieTalkie = true;
                player.speakingToWalkieTalkie = true;

                if (StartOfRound.Instance != null)
                {
                    StartOfRound.Instance.UpdatePlayerVoiceEffects();
                }

                onStartTalking.Invoke(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="_"></param>
        public void DisableWalkieLocal(PlayerControllerB _)
        {
            DisableWalkieServerRpc(GameNetworkManager.Instance.localPlayerController);
            IsActive = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        public void DisableWalkieServerRpc(NetworkBehaviourReference playerReference)
        {
            DisableWalkieClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void DisableWalkieClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                onStopTalking.Invoke(player);

                player.holdingWalkieTalkie = false;
                player.speakingToWalkieTalkie = false;

                if (StartOfRound.Instance != null)
                {
                    StartOfRound.Instance.UpdatePlayerVoiceEffects();
                }
            }
        }
    }
}