using itolib.Behaviours.Networking;
using UnityEngine;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayerElectrode : PlayerAttachable
    {
        /// <summary>
        ///     The speed at which to drain the player's items.
        /// </summary>
        /// <remarks>Can be set to <c>0</c> to disable automatic battery draining, or set to a negative value to charge batteries instead.</remarks>
        [Space(10.0f)]
        [Header("Player Electrode")]
        [Tooltip("The speed at which to drain the player's items. Can be set to '0' to disable automatic battery draining, or set to a negative value to "
            + "charge batteries instead.")]
        [SerializeField] private float batteryDrainMultiplier = 1.0f;

        /// <summary>
        ///     Whether to drain the player's held item or not.
        /// </summary>
        [Tooltip("Whether to drain the player's held item or not.")]
        [SerializeField] private bool affectHeldItem = true;

        /// <summary>
        ///     Whether to drain the player's pocketed items or not.
        /// </summary>
        [Tooltip("Whether to drain the player's pocketed items or not.")]
        [SerializeField] private bool affectPocketedItems = true;

        /// <summary>
        ///     Set some default values for draining purposes.
        /// </summary>
        protected override void Reset()
        {
            // Attaching locally is recommended for multiple players to be able to be drained.
            attachLocally = true;
            detachOnExit = true;

            base.Reset();
        }

        /// <summary>
        ///     Attach if the player is alive and enabled.
        ///     Detach if the player is dead or disabled.
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead && player.isActiveAndEnabled;
            detachCondition = player => player.isPlayerDead || !player.isActiveAndEnabled;
        }

        /// <summary>
        ///     Handle periodic item battery draining.
        /// </summary>
        protected override void Update()
        {
            // Check if local player is attached, and batteries are set to be drained every frame.
            if (localPlayerAttached && attachedPlayer != null && batteryDrainMultiplier != 0.0f)
            {
                // Check if player should have their held item's battery drained.
                if (affectHeldItem)
                {
                    // Obtain player's held item, if there is one.
                    GrabbableObject? item = attachedPlayer.currentlyHeldObjectServer;

                    // Check if held item has a battery and can be charged.
                    if (item != null && item.itemProperties != null && item.itemProperties.requiresBattery && item.insertedBattery != null)
                    {
                        // Drain held item battery, relative to the item's total charge.
                        DrainBattery(item.insertedBattery, batteryDrainMultiplier * Time.deltaTime / item.itemProperties.batteryUsage);
                    }
                }

                // Check if player should have their pocketed items' battery drained.
                if (affectPocketedItems)
                {
                    // Look through the player's item slots.
                    for (int i = 0; i < attachedPlayer.ItemSlots.Length; i++)
                    {
                        // Obtain pocketed item at the current slot, if there is one.
                        GrabbableObject? item = attachedPlayer.ItemSlots[i];

                        // Check if pocketed item has a battery and can be charged.
                        if (item != null && item.itemProperties != null && item.itemProperties.requiresBattery && item.insertedBattery != null
                            && attachedPlayer.currentlyHeldObjectServer != item)
                        {
                            // Drain pocketed item battery, relative to the item's total charge.
                            DrainBattery(item.insertedBattery, batteryDrainMultiplier * Time.deltaTime / item.itemProperties.batteryUsage);
                        }
                    }
                }
            }

            base.Update();
        }

        /// <summary>
        ///     Drain item held by the attached player.
        /// </summary>
        /// <remarks>Can be given a negative value to charge the item instead.</remarks>
        /// <param name="percentage">Percentage of battery charge to drain.</param>
        public void DrainHeldItem(float percentage)
        {
            // Check if local player is attached.
            if (localPlayerAttached && attachedPlayer != null)
            {
                // Obtain player's held item.
                GrabbableObject? item = attachedPlayer.currentlyHeldObjectServer;

                // Check if held item has a battery and can be charged.
                if (item != null && item.itemProperties != null && item.itemProperties.requiresBattery && item.insertedBattery != null)
                {
                    // Drain held item battery by the specified percentage.
                    DrainBattery(item.insertedBattery, percentage * 0.01f);
                }
            }
        }

        /// <summary>
        ///     Drain items pocketed by the attached player.
        /// </summary>
        /// <remarks>Can be given a negative value to charge the item instead.</remarks>
        /// <param name="percentage">Percentage of battery charge to drain.</param>
        public void DrainPocketedItems(float percentage)
        {
            // Check if local player is attached.
            if (localPlayerAttached && attachedPlayer != null)
            {
                // Look through the player's item slots.
                for (int i = 0; i < attachedPlayer.ItemSlots.Length; i++)
                {
                    // Obtain pocketed item at the current slot.
                    GrabbableObject? item = attachedPlayer.ItemSlots[i];

                    // Check if pocketed item has a battery and can be charged.
                    if (item != null && item.itemProperties != null && item.itemProperties.requiresBattery && item.insertedBattery != null
                        && attachedPlayer.currentlyHeldObjectServer != item)
                    {
                        // Drain held item battery by the specified percentage.
                        DrainBattery(item.insertedBattery, percentage * 0.01f);
                    }
                }
            }
        }

        /// <summary>
        ///     Drain all items present in the attached player's inventory.
        /// </summary>
        /// <remarks>Can be given a negative value to charge the item instead.</remarks>
        /// <param name="percentage">Percentage of battery charge to drain.</param>
        public void DrainAllItems(float percentage)
        {
            // Drain held item by the specified percentage.
            DrainHeldItem(percentage);

            // Drain pocketed items by the specified percentage.
            DrainPocketedItems(percentage);
        }

        /// <summary>
        ///     Drain a given <c>Battery</c> by a specified amount.
        /// </summary>
        /// <param name="battery">The <c>Battery</c> to drain, pertaining to a chargeable item.</param>
        /// <param name="amount">The amount to drain from the <c>Battery</c>.</param>
        private static void DrainBattery(Battery battery, float amount)
        {
            // Set battery's charge value, clamped between 0 and 1.
            battery.charge = Mathf.Clamp01(battery.charge - amount);
        }
    }
}