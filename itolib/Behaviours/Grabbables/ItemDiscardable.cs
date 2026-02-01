using GameNetcodeStuff;
using itolib.Extensions;
using itolib.Util;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     Adds <i>discardability</i> to any <c>GrabbableObject</c>.
    /// </summary>
    public class ItemDiscardable : NetworkBehaviour
    {
        /// <summary>
        ///     Item to target for discarding.
        /// </summary>
        [Header("Item Discardable")]
        [Tooltip("Item to target for discarding.")]
        [SerializeField] private GrabbableObject? item;

        /// <summary>
        ///     Parent <c>NetworkObject</c> to attempt to despawn once discarded, if set to despawn after discarding.
        /// </summary>
        [Tooltip("Parent NetworkObject to attempt to despawn once discarded, if set to despawn after discarding.")]
        [SerializeField] private NetworkObject? parentNetworkObject;

        /// <summary>
        ///     Whether to disable grabbing the item after being discarded or not.
        /// </summary>
        [Tooltip("Whether to disable grabbing the item after being discarded or not.")]
        [SerializeField] private bool disableGrabOnDiscard = false;

        /// <summary>
        ///     Callback invoked when the item is discarded.
        /// </summary>
        [Space(5.0f)]
        [Header("Events")]
        [Tooltip("Callback invoked when the item is discarded.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDiscardItem = new();

        /// <summary>
        ///     Callback invoked when the item is discarded while held by the player.
        /// </summary>
        [Tooltip("Callback invoked when the item is discarded while held by the player.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDiscardItemHeld = new();

        /// <summary>
        ///     Callback invoked when the item is discarded while pocketed by the player.
        /// </summary>
        [Tooltip("Callback invoked when the item is discarded while pocketed by the player.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDiscardItemPocketed = new();

        /// <summary>
        ///     Callback invoked when the item is discarded from dropping all items in the player's inventory.
        /// </summary>
        [Tooltip("Callback invoked when the item is discarded from dropping all items in the player's inventory.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDiscardItemsAll = new();

        /// <summary>
        ///     Whether to despawn the item after being discarded or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Despawn")]
        [Tooltip("Whether to despawn the item after being discarded or not.")]
        [SerializeField] private bool despawnOnDiscard = false;

        /// <summary>
        ///     Additional timer until the item is despawned, in seconds.
        /// </summary>
        /// <remarks><b>NOTE:</b> Should be used with <c>disableGrabOnDiscard</c> enabled, or else it could despawn while in the player's inventory.</remarks>
        [Tooltip("Additional timer until the item is despawned, in seconds. NOTE: Should be used with 'disableGrabOnDiscard' enabled, or else it could "
            + "despawn while in the player's inventory.")]
        [Min(0.0f)]
        [SerializeField] private float despawnTimer = 0.0f;

        /// <summary>
        ///     Whether to 'hide' the item instead of despawning it or not.
        /// </summary>
        /// <remarks><b>NOTE:</b>Vanilla items that destroy themselves (e.g. <c>Easter Egg</c>) are simply deactivated until despawning at the end of the round.</remarks>
        [Tooltip("Whether to 'hide' the item instead of despawning it or not. NOTE: Vanilla items that destroy themselves (e.g. Easter Egg) are simply deactivated "
            + "until despawning at the end of the round.")]
        [SerializeField] private bool despawnOnlyHides = true;

        /// <summary>
        ///     Callback invoked when the despawn timer for the item starts.
        /// </summary>
        [Space(5.0f)]
        [Header("Events")]
        [Tooltip("Callback invoked when the despawn timer for the item starts.")]
        [SerializeField] private UnityEvent onDespawnTimerStart = new();

        /// <summary>
        ///     Callback invoked when the despawn timer for the item ends.
        /// </summary>
        [Tooltip("Callback invoked when the despawn timer for the item ends.")]
        [SerializeField] private UnityEvent onDespawnTimerEnd = new();

        /// <summary>
        ///     Attempt to find a <c>GrabbableObject</c> to discard, if missing.
        /// </summary>
        private void Awake()
        {
            // Make sure the item field is not blank.
            if (item == null && !TryGetComponent(out item))
            {
                Plugin.StaticLogger.LogWarning($"Could not find GrabbableObject for ItemDiscardable component in GameObject '{gameObject.name}'.");
                enabled = false;

                return;
            }
        }

        /// <summary>
        ///     Forcibly discard the item from the player's inventory by specifically targeting it.
        /// </summary>
        public void ForceDropItem()
        {
            ForceDropItem(dropAll: false);
        }

        /// <summary>
        ///     Forcibly discard the item from the player's inventory by dropping all their items.
        /// </summary>
        public void ForceDropItems()
        {
            ForceDropItem(dropAll: true);
        }

        /// <summary>
        ///     Forcibly discard the item from the player's inventory.
        /// </summary>
        /// <param name="dropAll">Whether all other items should be dropped or not.</param>
        private void ForceDropItem(bool dropAll)
        {
            // Check if item is held by the local player.
            if (item == null || item.playerHeldBy == null || !item.playerHeldBy.IsLocalClient())
            {
                return;
            }

            // Obtain slot the item is in, or '-1' if dropping all items.
            int slot = !dropAll ? Array.IndexOf(item.playerHeldBy.ItemSlots, item) : -1;

            // Obtain whether the item is currently held by the player or not.
            bool inHand = !dropAll && !item.playerHeldBy.throwingObject && item.playerHeldBy.isHoldingObject
                && item.playerHeldBy.currentItemSlot == slot && item.playerHeldBy.currentlyHeldObjectServer == item;

            // Discard item on the local client.
            ForceDropItemLocal(item.playerHeldBy, slot, inHand, dropAll);

            if (IsSpawned)
            {
                // Send item being discarded to all other clients.
                ForceDropItemRpc(item.playerHeldBy, slot, inHand, dropAll);
            }
        }

        /// <summary>
        ///     Forcibly discard the item from the player's inventory for all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the player holding the item to discard.</param>
        /// <param name="slot">Slot in the player's inventory the item to discard is in.</param>
        /// <param name="inHand">Whether the player is holding the item to discard or not.</param>
        /// <param name="dropAll">Whether all other items should be dropped or not.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void ForceDropItemRpc(NetworkBehaviourReference playerReference, int slot, bool inHand, bool dropAll)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Discard item on the local client.
                ForceDropItemLocal(player, slot, inHand, dropAll);
            }
        }

        /// <summary>
        ///     Forcibly discard the item from the player's inventory for the local client.
        /// </summary>
        /// <param name="player">Player holding the item to discard.</param>
        /// <param name="slot">Slot in the player's inventory the item to discard is in.</param>
        /// <param name="inHand">Whether the player is holding the item to discard or not.</param>
        /// <param name="dropAll">Whether all other items should be dropped or not.</param>
        private void ForceDropItemLocal(PlayerControllerB player, int slot, bool inHand, bool dropAll)
        {
            // Check if item to discard exists.
            if (item == null)
            {
                return;
            }

            // Check if item should be despawned after being discarded.
            if (despawnOnDiscard)
            {
                // Start timer for despawning the item.
                _ = StartCoroutine(DespawnItemDelayed());
            }

            // Check if item should be set as ungrabbable after being discarded.
            if (disableGrabOnDiscard || despawnOnDiscard)
            {
                item.grabbable = false;
                item.grabbableToEnemies = false;
            }

            if (dropAll)
            {
                // Discard all items the player has.
                player.DropAllHeldItems();

                // Invoke all items dropped event.
                onDiscardItemsAll.Invoke(player);
            }
            else if (inHand)
            {
                // Discard the player's currently held item.
                player.DiscardHeldObject();

                // Invoke held item dropped event.
                onDiscardItemHeld.Invoke(player);
            }
            else
            {
                // Discard the item in the specified inventory slot.
                player.DiscardHeldObject(slot);

                // Invoke pocketed item dropped event.
                onDiscardItemPocketed.Invoke(player);
            }

            // Invoke item discard event.
            onDiscardItem.Invoke(player);
        }

        /// <summary>
        ///     Despawn item, or simply hide it (like vanilla).
        /// </summary>
        private void DespawnItem()
        {
            // Check if item to despawn exists.
            if (item == null)
            {
                return;
            }

            // Check if item should be hidden (vanilla 'despawning').
            if (despawnOnlyHides)
            {
                item.deactivated = true;

                // Destroy the item's radar map icon.
                if (item.radarIcon != null)
                {
                    Destroy(item.radarIcon.gameObject);
                }

                // Disable any Renderers present in the item.
                Renderer[] renderers = item.GetComponentsInChildren<Renderer>(includeInactive: false);
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;
                    // Destroy(renderers[i]);
                }

                // Disable any Colliders present in the item.
                Collider[] colliders = item.GetComponentsInChildren<Collider>(includeInactive: false);
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = false;
                    // Destroy(colliders[i]);
                }

                return;
            }

            if (RoundManager.Instance != null)
            {
                // Remove item from the list of spawned objects, if present.
                _ = RoundManager.Instance.spawnedSyncedObjects.Remove(item.gameObject);
            }

            if (IsHost && parentNetworkObject != null && parentNetworkObject.IsSpawned)
            {
                // Actually despawn the item.
                parentNetworkObject.Despawn(true);
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to despawn the item after a specified amount of time passes.
        /// </summary>
        private IEnumerator DespawnItemDelayed()
        {
            // Invoke despawn timer start event.
            onDespawnTimerStart.Invoke();
            yield return Yielders.WaitForSeconds(despawnTimer);

            // Invoke despawn timer end event.
            onDespawnTimerEnd.Invoke();
            yield return Yielders.WaitForEndOfFrame;

            // Despawn item.
            DespawnItem();
        }

        /// <summary>
        ///     Switch item to discard.
        /// </summary>
        /// <param name="replacingItem">Item to target for discarding.</param>
        public void SwitchDiscardItem(GrabbableObject replacingItem)
        {
            // Check if already set as the item to discard.
            if (item == replacingItem)
            {
                return;
            }

            // Switch item to discard on the local client.
            SwitchDiscardItemLocal(replacingItem);

            if (IsSpawned)
            {
                // Send item to discard switch to all other clients.
                SwitchDiscardItemRpc(replacingItem);
            }
        }

        /// <summary>
        ///     Switch item to discard for all other clients.
        /// </summary>
        /// <param name="itemReference">Network reference of the item to target for discarding.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SwitchDiscardItemRpc(NetworkBehaviourReference itemReference)
        {
            if (itemReference.TryGet(out GrabbableObject replacingItem))
            {
                // Switch item to discard on the local client.
                SwitchDiscardItemLocal(replacingItem);
            }
        }

        /// <summary>
        ///     Switch item to discard for the local client.
        /// </summary>
        /// <param name="replacingItem">Item to target for discarding.</param>
        private void SwitchDiscardItemLocal(GrabbableObject replacingItem)
        {
            if (item != null && item.TryGetComponent(out parentNetworkObject) && parentNetworkObject!.IsSpawned)
            {
                // Switch item to discard.
                item = replacingItem;
            }
        }
    }
}