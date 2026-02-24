using GameNetcodeStuff;
using itolib.Enums;
using itolib.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     Adds <i>wearability</i> to an <i>eventful</i> <c>GrabbableObject</c>. Mimics <c>BeltBagItem</c>, when pocketed.
    /// </summary>
    public class ItemWearable : NetworkBehaviour
    {
        /// <summary>
        ///     Item with <i>eventful</i> properties (e.g. <c>ItemGrabbable</c> or <c>EventfulApparatus</c>).
        /// </summary>
        [Header("Item Wearable")]
        [Tooltip("Item with 'eventful' properties (e.g. 'ItemGrabbable' or 'EventfulApparatus').")]
        [SerializeField] private GrabbableObject? item;

        /// <summary>
        ///     What the wearable should attach to when pocketed.
        /// </summary>
        [Tooltip("What the wearable should attach to when pocketed.")]
        [SerializeField] private WearablePosition wearPosition = WearablePosition.Custom;

        /// <summary>
        ///     Path of the bone to attach the wearable to when pocketed, if set to a custom position.
        /// </summary>
        [Tooltip("Path of the bone to attach the wearable to when pocketed, if set to a custom position.")]
        [SerializeField] private string customBone = string.Empty;

        /// <summary>
        ///     <c>Transform</c> to apply the position and rotation offsets to.
        /// </summary>
        [Header("Offset")]
        [Tooltip("Transform to apply the position and rotation offsets to.")]
        [SerializeField] private Transform? applyOffsetTo;

        /// <summary>
        ///     Position offset to apply to the wearable when equipped.
        /// </summary>
        [Tooltip("Position offset to apply to the wearable when equipped.")]
        [SerializeField] private Vector3 wearPositionOffset = Vector3.zero;

        /// <summary>
        ///     Rotation offset to apply to the wearable when equipped.
        /// </summary>
        [Tooltip("Rotation offset to apply to the wearable when equipped.")]
        [SerializeField] private Quaternion wearRotationOffset = Quaternion.identity;

        /// <summary>
        ///     Default local position of the wearable to reset back to when unequipping.
        /// </summary>
        private Vector3 initialPosition = Vector3.zero;

        /// <summary>
        ///     Default local rotation of the wearable to reset back to when unequipping.
        /// </summary>
        private Quaternion initialRotation = Quaternion.identity;

        /// <summary>
        ///     <c>Transform</c> that the wearable will be attached to.
        /// </summary>
        private Transform? boneToAttachTo;

        /// <summary>
        ///     Initialize stuff required to mimic a <c>BeltBagItem</c>'s wearability.
        /// </summary>
        private void Awake()
        {
            // Make sure the item field implements IEventfulItem.
            if ((item == null && !TryGetComponent(out item)) || item is not IEventfulItem eventfulItem)
            {
                Plugin.StaticLogger.LogWarning($"Could not find IEventfulItem for ItemWearable component in GameObject '{gameObject.name}'.");
                enabled = false;

                return;
            }

            // Wearables are equipped when pocketed, thus should not be hidden.
            eventfulItem.HideOnPocket = false;

            // Subscribe to related event callbacks:
            eventfulItem.OnDiscardEarly.AddListener(OnDiscardEarly);
            eventfulItem.OnEquip.AddListener(OnEquip);
            eventfulItem.OnGrab.AddListener(SetWearablePosition);
            eventfulItem.OnPocket.AddListener(OnPocket);
            // ...
        }

        /// <summary>
        ///     Save local position and rotation for the wearable when unequipped.
        /// </summary>
        private void Start()
        {
            if (applyOffsetTo != null)
            {
                applyOffsetTo.GetLocalPositionAndRotation(out initialPosition, out initialRotation);
            }
        }

        /// <summary>
        ///     Set <c>Transform</c> for the wearable to attach to when equipped. Updates when grabbed by a player.
        /// </summary>
        private void SetWearablePosition()
        {
            if (item != null && item.playerHeldBy != null && item.playerHeldBy.IsOwner)
            {
                // Set Transform to attach to on the local client.
                SetWearablePositionLocal(item.playerHeldBy);

                if (IsSpawned)
                {
                    // Send Transform to attach to to all other clients.
                    SetWearablePositionRpc(item.playerHeldBy);
                }
            }
        }

        /// <summary>
        ///     Set <c>Transform</c> for the wearable to attach to for all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the player that grabbed the wearable.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SetWearablePositionRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Set Transform to attach to on the local client.
                SetWearablePositionLocal(player);
            }
        }

        /// <summary>
        ///     Set <c>Transform</c> for the wearable to attach to for the local client.
        /// </summary>
        /// <param name="player">Player that grabbed the wearable.</param>
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
        ///     Unequip, reset, and remove parent upon dropping the wearable.
        /// </summary>
        private void OnDiscardEarly()
        {
            OnEquip();

            if (item != null)
            {
                item.parentObject = null;
            }
        }

        /// <summary>
        ///     Unequip and reset upon holding the wearable.
        /// </summary>
        private void OnEquip()
        {
            EquipWearable(reset: true);
        }

        /// <summary>
        ///     Equip upon pocketing the wearable.
        /// </summary>
        private void OnPocket()
        {
            EquipWearable(reset: false);
        }

        /// <summary>
        ///     Equip wearable item for the player that has it. Updates when the item is grabbed, dropped, or pocketed.
        /// </summary>
        /// <param name="reset">Whether the wearable is being unequipped or not.</param>
        private void EquipWearable(bool reset = false)
        {
            if (item != null && item.playerHeldBy != null && item.playerHeldBy.IsOwner)
            {
                // Equip wearable item on the local client.
                EquipWearableLocal(item.playerHeldBy, reset);

                if (IsSpawned)
                {
                    // Send wearable item being equipped to all other clients.
                    EquipWearableRpc(item.playerHeldBy, reset);
                }
            }
        }

        /// <summary>
        ///     Equip wearable item for the player that has it for all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the player equipping or unequipping the wearable.</param>
        /// <param name="reset">Whether the wearable is being unequipped or not.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void EquipWearableRpc(NetworkBehaviourReference playerReference, bool reset = false)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Equip wearable item on the local client.
                EquipWearableLocal(player, reset);
            }
        }

        /// <summary>
        ///     Equip wearable item for the local client.
        /// </summary>
        /// <param name="player">Player equipping or unequipping the wearable.</param>
        /// <param name="reset">Whether the wearable is being unequipped or not.</param>
        private void EquipWearableLocal(PlayerControllerB player, bool reset = false)
        {
            if (item == null)
            {
                return;
            }

            if (!reset)
            {
                player.IsInspectingItem = false;
                player.equippedUsableItemQE = false;

                item.isPocketed = true;
                item.parentObject = boneToAttachTo;
            }
            else
            {
                item.parentObject = player.IsOwner ? player.localItemHolder : player.serverItemHolder;
            }

            if (applyOffsetTo != null)
            {
                applyOffsetTo.SetLocalPositionAndRotation(!reset ? wearPositionOffset : initialPosition,
                    !reset ? wearRotationOffset : initialRotation);
            }
        }
    }
}