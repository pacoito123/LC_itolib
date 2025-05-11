using GameNetcodeStuff;
using itolib.Enums;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [RequireComponent(typeof(ItemGrabbable))]
    public class ItemWearable : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public Vector3 InitialPosition { get; private set; } = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Quaternion InitialRotation { get; private set; } = Quaternion.identity;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Transform? BoneToAttachTo { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Wearable")]
        [Tooltip("")]
        public ItemGrabbable item = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public WearablePosition wearPosition = WearablePosition.Custom;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string customBone = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Offset")]
        [Tooltip("")]
        public Transform? applyOffsetTo;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 wearPositionOffset = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Quaternion wearRotationOffset = Quaternion.identity;

        private void Awake()
        {
            item ??= GetComponent<ItemGrabbable>();
            item.hideOnPocket = false;

            item.onDiscard?.AddListener(DiscardItem);
            item.onEquip?.AddListener(EquipItem);
            item.onGrab?.AddListener(GrabItem);
            item.onPocket?.AddListener(PocketItem);

            if (applyOffsetTo != null)
            {
                InitialPosition = applyOffsetTo.localPosition;
                InitialRotation = applyOffsetTo.localRotation;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void DiscardItem()
        {
            if (BoneToAttachTo != null)
            {
                applyOffsetTo?.SetLocalPositionAndRotation(InitialPosition, InitialRotation);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void EquipItem()
        {
            item.parentObject = item.playerHeldBy.actualClientId == GameNetworkManager.Instance.localPlayerController.actualClientId
                ? item.playerHeldBy.localItemHolder.transform : item.playerHeldBy.serverItemHolder.transform;

            if (BoneToAttachTo != null)
            {
                applyOffsetTo?.SetLocalPositionAndRotation(InitialPosition, InitialRotation);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void GrabItem()
        {
            if (item.IsOwner && item.playerHeldBy != null)
            {
                PlayerControllerB player = item.playerHeldBy;

                switch (wearPosition)
                {
                    case WearablePosition.Custom:
                        BoneToAttachTo = player.playerBodyAnimator.transform.Find(customBone);
                        break;
                    case WearablePosition.Head:
                        BoneToAttachTo = player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId
                            ? player.headCostumeContainer : player.headCostumeContainerLocal;
                        // BoneToAttachTo = player.headCostumeContainer;
                        break;
                    case WearablePosition.Belt:
                        BoneToAttachTo = player.lowerTorsoCostumeContainer;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PocketItem()
        {
            if (item.IsOwner && item.playerHeldBy != null)
            {
                item.playerHeldBy.IsInspectingItem = false;
                item.playerHeldBy.equippedUsableItemQE = false;
            }

            item.isPocketed = true;

            if (BoneToAttachTo != null)
            {
                item.parentObject = BoneToAttachTo;

                applyOffsetTo?.SetLocalPositionAndRotation(wearPositionOffset, wearRotationOffset);
            }
        }
    }
}