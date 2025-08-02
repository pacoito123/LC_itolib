using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemWearable : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Wearable")]
        [Tooltip("")]
        [SerializeField] private GrabbableObject item = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private WearablePosition wearPosition = WearablePosition.Custom;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private string customBone = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Offset")]
        [Tooltip("")]
        [SerializeField] private Transform? applyOffsetTo;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Vector3 wearPositionOffset = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Quaternion wearRotationOffset = Quaternion.identity;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Vector3 initialPosition = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Quaternion initialRotation = Quaternion.identity;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Transform? boneToAttachTo;

        /// <summary>
        ///     TODO.
        /// </summary>
        private IEventfulItem? eventfulSelf;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (item == null || !TryGetComponent(out item) || item is not IEventfulItem eventfulItem)
            {
                // TODO: Log warning
                enabled = false;

                return;
            }

            eventfulSelf = eventfulItem;

            eventfulSelf.HideOnPocket = false;

            eventfulItem.OnDiscardEarly.AddListener(OnDiscardEarly);
            eventfulItem.OnEquip.AddListener(OnEquip);
            eventfulItem.OnGrab.AddListener(SetWearablePosition);
            eventfulItem.OnPocket.AddListener(OnPocket);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Start()
        {
            if (applyOffsetTo != null)
            {
                initialPosition = applyOffsetTo.localPosition;
                initialRotation = applyOffsetTo.localRotation;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void SetWearablePosition()
        {
            if (item.playerHeldBy != null && item.playerHeldBy.IsOwner)
            {
                SetWearablePositionLocal(item.playerHeldBy);
                SetWearablePositionServerRpc(item.playerHeldBy);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        private void SetWearablePositionServerRpc(NetworkBehaviourReference playerReference)
        {
            SetWearablePositionClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        private void SetWearablePositionClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SetWearablePositionLocal(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        private void SetWearablePositionLocal(PlayerControllerB player)
        {
            switch (wearPosition)
            {
                case WearablePosition.Custom:
                    boneToAttachTo = player.playerBodyAnimator.transform.Find(customBone);
                    break;
                case WearablePosition.Head:
                    boneToAttachTo = player.IsOwner ? player.headCostumeContainerLocal : player.headCostumeContainer;
                    break;
                case WearablePosition.Belt:
                    boneToAttachTo = player.lowerTorsoCostumeContainer;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnDiscardEarly()
        {
            OnEquip();

            item.parentObject = null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEquip()
        {
            EquipWearable(reset: true);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnPocket()
        {
            EquipWearable(reset: false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="reset"></param>
        private void EquipWearable(bool reset = false)
        {
            if (item.playerHeldBy != null && item.playerHeldBy.IsOwner)
            {
                EquipWearableLocal(item.playerHeldBy, reset);
                EquipWearableServerRpc(item.playerHeldBy, reset);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="reset"></param>
        [ServerRpc(RequireOwnership = false)]
        private void EquipWearableServerRpc(NetworkBehaviourReference playerReference, bool reset = false)
        {
            EquipWearableClientRpc(playerReference, reset);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="reset"></param>
        [ClientRpc]
        private void EquipWearableClientRpc(NetworkBehaviourReference playerReference, bool reset = false)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                EquipWearableLocal(player, reset);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reset"></param>
        private void EquipWearableLocal(PlayerControllerB player, bool reset = false)
        {
            if (!reset)
            {
                player.IsInspectingItem = false;
                player.equippedUsableItemQE = false;

                item.isPocketed = true;
                item.parentObject = boneToAttachTo;
            }
            else
            {
                item.parentObject = player.IsOwner ? player.localItemHolder.transform : player.serverItemHolder.transform;
            }

            if (applyOffsetTo != null)
            {
                applyOffsetTo.SetLocalPositionAndRotation(!reset ? wearPositionOffset : initialPosition,
                    !reset ? wearRotationOffset : initialRotation);
            }
        }
    }
}