using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
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

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            if (item == null && !TryGetComponent(out item))
            {
                // TODO: Log warning
                enabled = false;

                return;
            }

            item.hideOnPocket = false;

            item.onDiscardEarly.AddListener(OnDiscardEarly);
            item.onEquip.AddListener(OnEquip);
            item.onGrab.AddListener(SetWearablePosition);
            item.onPocket.AddListener(OnPocket);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            if (applyOffsetTo != null)
            {
                InitialPosition = applyOffsetTo.localPosition;
                InitialRotation = applyOffsetTo.localRotation;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SetWearablePosition()
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
        public void SetWearablePositionServerRpc(NetworkBehaviourReference playerReference)
        {
            SetWearablePositionClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        public void SetWearablePositionClientRpc(NetworkBehaviourReference playerReference)
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
                    BoneToAttachTo = player.playerBodyAnimator.transform.Find(customBone);
                    break;
                case WearablePosition.Head:
                    BoneToAttachTo = player.IsOwner ? player.headCostumeContainerLocal : player.headCostumeContainer;
                    break;
                case WearablePosition.Belt:
                    BoneToAttachTo = player.lowerTorsoCostumeContainer;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDiscardEarly()
        {
            OnEquip();

            item.parentObject = null;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEquip()
        {
            EquipWearable(reset: true);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnPocket()
        {
            EquipWearable(reset: false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="reset"></param>
        public void EquipWearable(bool reset = false)
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
        public void EquipWearableServerRpc(NetworkBehaviourReference playerReference, bool reset = false)
        {
            EquipWearableClientRpc(playerReference, reset);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="reset"></param>
        [ClientRpc]
        public void EquipWearableClientRpc(NetworkBehaviourReference playerReference, bool reset = false)
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
                item.parentObject = BoneToAttachTo;
            }
            else
            {
                item.parentObject = player.IsOwner ? player.localItemHolder.transform : player.serverItemHolder.transform;
            }

            if (applyOffsetTo != null)
            {
                applyOffsetTo.SetLocalPositionAndRotation(!reset ? wearPositionOffset : InitialPosition,
                    !reset ? wearRotationOffset : InitialRotation);
            }
        }
    }
}