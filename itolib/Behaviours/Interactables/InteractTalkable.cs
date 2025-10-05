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

            if (IsSpawned)
            {
                EnableWalkieRpc(GameNetworkManager.Instance.localPlayerController);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void EnableWalkieRpc(NetworkBehaviourReference playerReference)
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

            if (IsSpawned)
            {
                DisableWalkieRpc(GameNetworkManager.Instance.localPlayerController);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void DisableWalkieRpc(NetworkBehaviourReference playerReference)
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