using GameNetcodeStuff;
using itolib.Extensions;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemDiscardable : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Discardable")]
        [Tooltip("")]
        [SerializeField] private GrabbableObject item = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private NetworkObject parentNetworkObject = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool disableGrabOnDiscard = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDiscardItem = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDiscardItemHeld = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDiscardItemPocketed = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onDiscardItemsAll = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Despawn")]
        [Tooltip("")]
        [SerializeField] private bool despawnOnDiscard = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] private float despawnTimer = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool despawnOnlyHides = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onDespawnTimerStart = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onDespawnTimerEnd = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (item == null || !TryGetComponent(out item))
            {
                // TODO: Log warning
                enabled = false;

                return;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ForceDropItem()
        {
            ForceDropItem(dropAll: false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ForceDropItems()
        {
            ForceDropItem(dropAll: true);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dropAll"></param>
        private void ForceDropItem(bool dropAll)
        {
            if (item.playerHeldBy == null || !item.playerHeldBy.IsLocalClient())
            {
                return;
            }

            PlayerControllerB player = item.playerHeldBy;

            if (dropAll)
            {
                player.DropAllHeldItemsAndSync();

                onDiscardItem?.Invoke(player);
                onDiscardItemsAll?.Invoke(player);

                return;
            }
            else if (!player.throwingObject && player.isHoldingObject && player.currentlyHeldObjectServer == item)
            {
                player.DiscardHeldObject();

                onDiscardItem?.Invoke(player);
                onDiscardItemHeld?.Invoke(player);

                return;
            }

            int slot = -1;

            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                if (player.ItemSlots[i] == item)
                {
                    slot = i;

                    break;
                }
            }

            ForceDropItemLocal(player, slot);
            ForceDropItemServerRpc(player, slot);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="slot"></param>
        [ServerRpc(RequireOwnership = false)]
        private void ForceDropItemServerRpc(NetworkBehaviourReference playerReference, int slot)
        {
            ForceDropItemClientRpc(playerReference, slot);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="slot"></param>
        [ClientRpc]
        private void ForceDropItemClientRpc(NetworkBehaviourReference playerReference, int slot)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                ForceDropItemLocal(player, slot);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="slot"></param>
        private void ForceDropItemLocal(PlayerControllerB player, int slot)
        {
            item.parentObject = null;
            item.heldByPlayerOnServer = false;

            if (item.isInElevator)
            {
                transform.SetParent(player.playersManager.elevatorTransform, true);
            }
            else
            {
                transform.SetParent(player.playersManager.propsContainer, true);
            }

            player.SetItemInElevator(player.isInHangarShipRoom, player.isInElevator, item);

            item.EnablePhysics(true);
            item.EnableItemMeshes(true);

            transform.localScale = item.originalScale;

            item.isHeld = false;
            item.isPocketed = false;

            item.startFallingPosition = transform.parent.InverseTransformPoint(transform.position);
            item.FallToGround(true, false, Vector3.zero);
            item.fallTime = Random.Range(-0.3f, 0.05f);

            if (slot > -1)
            {
                player.ItemSlots[slot] = null;
            }

            if (player.IsLocalClient())
            {
                item.DiscardItemOnClient();

                if (slot > -1)
                {
                    HUDManager.Instance.itemSlotIcons[slot].enabled = false;
                }
            }
            else if (!item.itemProperties.syncDiscardFunction)
            {
                item.playerHeldBy = null;
            }

            if (disableGrabOnDiscard || despawnOnDiscard)
            {
                item.grabbable = false;
                item.grabbableToEnemies = false;
            }

            onDiscardItem?.Invoke(player);
            onDiscardItemPocketed?.Invoke(player);

            if (despawnOnDiscard)
            {
                _ = StartCoroutine(DespawnItemDelayed());
            }
        }

        private void DespawnItem()
        {
            // Vanilla item "despawning":
            item.deactivated = true;

            if (item.radarIcon != null)
            {
                Destroy(item.radarIcon.gameObject);
            }

            foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
            {
                // Destroy(renderer);
                renderer.enabled = false;
            }

            foreach (Collider collider in item.GetComponentsInChildren<Collider>())
            {
                // Destroy(collider);
                collider.enabled = false;
            }
            // ...

            if (!despawnOnlyHides)
            {
                if (RoundManager.Instance != null)
                {
                    _ = RoundManager.Instance.spawnedSyncedObjects.Remove(item.gameObject);
                }

                if (IsHost && parentNetworkObject != null && parentNetworkObject.IsSpawned)
                {
                    parentNetworkObject.Despawn(true);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private IEnumerator DespawnItemDelayed()
        {
            onDespawnTimerStart.Invoke();
            yield return new WaitForSeconds(despawnTimer);

            onDespawnTimerEnd.Invoke();
            yield return new WaitForEndOfFrame();

            DespawnItem();
        }
    }
}