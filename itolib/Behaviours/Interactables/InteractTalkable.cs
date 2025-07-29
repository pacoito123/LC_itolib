using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
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
        [Space(5.0f)]
        [Header("Interact Talkable")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onStartTalking = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onStopTalking = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool isActive;

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
        private void Awake()
        {
            holdingInteractEvent.AddListener(EnableWalkieLocal);
            onStopInteract.AddListener(DisableWalkieLocal);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="_"></param>
        private void EnableWalkieLocal(float _)
        {
            if (isActive)
            {
                return;
            }
            isActive = true;

            EnableWalkieServerRpc(GameNetworkManager.Instance.localPlayerController);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        private void EnableWalkieServerRpc(NetworkBehaviourReference playerReference)
        {
            EnableWalkieClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        private void EnableWalkieClientRpc(NetworkBehaviourReference playerReference)
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
        private void DisableWalkieLocal(PlayerControllerB _)
        {
            isActive = false;
            DisableWalkieServerRpc(GameNetworkManager.Instance.localPlayerController);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        private void DisableWalkieServerRpc(NetworkBehaviourReference playerReference)
        {
            DisableWalkieClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        private void DisableWalkieClientRpc(NetworkBehaviourReference playerReference)
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