using GameNetcodeStuff;
using itolib.Extensions;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Interactables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct Notification
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool AlreadySeen { get; private set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string headerText = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string bodyText = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool showOnce = false;

        /// <summary>
        ///     TODO.
        /// </summary> 
        public Notification() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SetNotificationSeen()
        {
            AlreadySeen = true; // TODO: Do without mutating struct. Whole separate notification system maybe?
        }
    }

    /// <summary>
    ///     Represents an InteractTrigger that can spawn an object when interacted with, for a price.
    /// </summary>
    public class InteractPurchasable : InteractTrigger // TODO: Needs more abstraction.
    {
        /// <summary>
        ///     Cached Terminal instance, to actually interact with the shared ship credits.
        /// </summary>
        public static Terminal? Terminal { get; private set; }

        /// <summary>
        ///     Prefab to spawn upon a successful transaction.
        /// </summary>
        [Space(5.0f)]
        [Header("Purchasable Object")]
        [Tooltip("Prefab to spawn upon a successful transaction.")]
        public GameObject? spawnPrefab;

        /// <summary>
        ///     Position and rotation of the purchasable object when spawned.
        /// </summary>
        [Tooltip("Position and rotation of the purchasable object when spawned.")]
        [SerializeField] private Transform? spawnTransform;

        /// <summary>
        ///     Credits required to spawn the purchasable object. Set to -1 to disable purchasing stuff at all.
        /// </summary>
        [Tooltip("Credits required to spawn the purchasable object.")]
        [Range(-1, 10000)]
        public int price = -1;

        /// <summary>
        ///     Callback invoked when a transaction is complete but the object has not yet been spawned.
        /// </summary>
        /// <remarks>
        ///     NOTE: Actions won't run using the instantiated object, but they can be used to modify the prefab on the host before spawning it, which can then
        ///     be synchronized with clients using NetworkVariables... Not ideal.
        /// </remarks>
        [Tooltip("Callback invoked when a transaction is complete but the object has not yet been spawned. NOTE: Actions won't run using the instantiated object, but they "
            + "can be used to modify the prefab on the host before spawning it, which can then be synchronized with clients using NetworkVariables... Not ideal.")]
        public UnityEvent onPurchase = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onPurchaseNotify = new();

        /// <summary>
        ///     TODO
        /// </summary>
        [Tooltip("")]
        public Notification purchaseNotification;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (Terminal == null)
            {
                // Cache Terminal instance if not already done.
                Terminal = FindObjectOfType<Terminal>();
            }

            // Add action to the interact callback.
            onInteract.AddListener(player =>
            {
                // Check if the local player was the one who interacted.
                if (player.IsLocalClient())
                {
                    // Attempt to purchase and spawn object.
                    RequestPurchaseServerRpc(player);
                }
            });
        }

        /// <summary>
        ///     Attempt to purchase and spawn instance of the purchasable object.
        /// </summary>
        /// <param name="playerReference">NetworkBehaviour reference of the player attempting the purchase.</param>
        [ServerRpc(RequireOwnership = false)]
        private void RequestPurchaseServerRpc(NetworkBehaviourReference playerReference)
        {
            if (spawnPrefab == null || spawnTransform == null || Terminal == null)
            {
                return;
            }

            // Return if group funds are insufficient for the purchase, or purchasing is disabled.
            if (price < 0 || Terminal.groupCredits < price)
            {
                // Send notification to the player who attempted the purchase.
                SendNotificationClientRpc(playerReference, price);

                return;
            }

            // Subtract price from the group credits and synchronize with all clients.
            Terminal.SyncGroupCreditsServerRpc(Terminal.groupCredits - price, -1);

            // Invoke purchase event.
            onPurchase.Invoke();

            // Instantiate purchasable object instance and spawn it on all clients. TODO: Handle through PrefabSpawner instead.
            GameObject purchasable = Instantiate(spawnPrefab, spawnTransform.position, spawnTransform.rotation, (RoundManager.Instance != null
                && RoundManager.Instance.mapPropsContainer != null) ? RoundManager.Instance.mapPropsContainer.transform : null);
            if (purchasable.TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.Spawn(true);
            }

            SendNotificationClientRpc(playerReference, success: true);
        }

        /// <summary>
        ///     Send a notification message to a specific player.
        /// </summary>
        /// <param name="playerReference">NetworkBehaviour reference of the player to receive the notification.</param>
        /// <param name="price">Price of the purchasable object.</param>
        /// <param name="success">Whether or not the purchase was succesful.</param>
        [ClientRpc]
        private void SendNotificationClientRpc(NetworkBehaviourReference playerReference, int price = -1, bool success = false)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && player.IsLocalClient())
            {
                if (success)
                {
                    if (!purchaseNotification.AlreadySeen)
                    {
                        /* HUDManager.Instance.DisplayTip(purchaseNotification.headerText.Replace("PRICE", $"{price}"),
                            purchaseNotification.bodyText.Replace("PRICE", $"{price}")); */
                        HUDManager.Instance.DisplayTip(purchaseNotification.headerText, purchaseNotification.bodyText);

                        if (purchaseNotification.showOnce)
                        {
                            onPurchaseNotify.Invoke();
                        }
                    }

                    return;
                }

                if (price >= 0)
                {
                    HUDManager.Instance.DisplayTip("Cannot afford purchase!", $"A minimum of {price} credits is required.");
                }
                else
                {
                    HUDManager.Instance.DisplayTip("Cannot purchase!", "This purchasable has been disabled by the host.");
                }
            }
        }
    }
}