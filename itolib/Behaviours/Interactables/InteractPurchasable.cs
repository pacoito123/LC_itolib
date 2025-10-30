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
        public bool showOnce;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public bool alreadySeen;

        /// <summary>
        ///     TODO.
        /// </summary> 
        public Notification() { }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SetNotificationSeen()
        {
            alreadySeen = true; // TODO: Do without mutating struct. Whole separate notification system maybe?
        }
    }

    /// <summary>
    ///     Represents an InteractTrigger that can perform a purchase when interacted with, if enough money is available.
    /// </summary>
    public class InteractPurchasable : InteractTrigger
    {
        /// <summary>
        ///     Cached Terminal instance, to actually interact with the shared ship credits.
        /// </summary>
        public static Terminal? Terminal { get; private set; }

        /// <summary>
        ///     Amount of credits required to perform the transaction. Set to -1 to disable purchasing altogether.
        /// </summary>
        [Space(5.0f)]
        [Header("Purchasable Object")]
        [Tooltip("Amount of credits required to perform the transaction. Set to -1 to disable purchasing altogether.")]
        [Range(-1, 10000)]
        public int price = -1;

        /// <summary>
        ///     Callback invoked when a transaction is complete but the object has not yet been spawned.
        /// </summary>
        [Tooltip("Callback invoked when a transaction is complete but the object has not yet been spawned.")]
        [SerializeField] private UnityEvent onPurchase = new();

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
                Terminal = FindFirstObjectByType<Terminal>(FindObjectsInactive.Exclude);
            }

            // Add action to the interact callback.
            onInteract.AddListener(player =>
            {
                // Check if the local player was the one who interacted.
                if (player.IsLocalClient())
                {
                    // Attempt to purchase and spawn object.
                    RequestPurchaseRpc();
                }
            });
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="interactable"></param>
        public void SetInteractable(bool interactable)
        {
            this.interactable = interactable;
        }

        /// <summary>
        ///     Attempt to purchase and spawn instance of the purchasable object.
        /// </summary>
        /// <param name="rpcParams"></param>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void RequestPurchaseRpc(RpcParams rpcParams = default)
        {
            if (Terminal == null)
            {
                return;
            }

            // Return if group funds are insufficient for the purchase, or purchasing is disabled.
            if (price < 0 || Terminal.groupCredits < price)
            {
                // Send notification to the player who attempted the purchase.
                SendNotificationRpc(price, success: false, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));

                return;
            }

            // Subtract price from the group credits and synchronize with all clients.
            Terminal.SyncGroupCreditsServerRpc(Terminal.groupCredits - price, -1);

            // Invoke purchase event.
            onPurchase.Invoke();

            // Send notification to the player who completed the purchase.
            SendNotificationRpc(price: -1, success: true, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        /// <summary>
        ///     Send a notification message to a specific player.
        /// </summary>
        /// <param name="price">Price of the purchasable object.</param>
        /// <param name="success">Whether or not the purchase was succesful.</param>
        /// <param name="rpcParams"></param>
        [Rpc(SendTo.SpecifiedInParams)]
        private void SendNotificationRpc(int price, bool success, RpcParams rpcParams)
        {
            if (HUDManager.Instance == null)
            {
                return;
            }

            if (success)
            {
                if (!purchaseNotification.alreadySeen)
                {
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