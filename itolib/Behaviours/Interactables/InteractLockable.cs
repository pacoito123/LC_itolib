using UnityEngine;

namespace itolib.Behaviours.Interactables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class InteractLockable : DoorLock
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public bool WasUnlockedLastFrame { get; private set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        public bool StartedLocked { get; private set; } = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Interact Lockable")]
        [Tooltip("")]
        public string doorHoverMessage = "Use door : [LMB]";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string doorUnlockMessage = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string doorLockedMessage = "Locked";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string doorLockedKeyMessage = "Use key: [ LMB ]";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string doorLockedKeyControllerMessage = "Use key: [R-trigger]";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string lockPickHoverMessage = "Locked (pickable)";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string lockPickUnlockMessage = "Picking lock";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string lockPickUnlockTimerMessage = "Picking lock: {0} sec.";

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void Awake()
        {
            base.Awake();

            if (isLocked)
            {
                StartedLocked = true;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="timeToLockPick"></param>
        public new void LockDoor(float timeToLockPick = 30.0f)
        {
            doorTrigger.interactable = false;
            doorTrigger.timeToHold = timeToLockPick;
            doorTrigger.timeToHoldSpeedMultiplier = 1.0f;
            doorTrigger.hoverTip = lockPickHoverMessage;
            doorTrigger.holdTip = lockPickUnlockMessage;
            isPickingLock = false;
            isLocked = true;

            if (navMeshObstacle != null)
            {
                navMeshObstacle.carving = true;
                navMeshObstacle.carveOnlyStationary = true;
            }

            if (twinDoor != null)
            {
                twinDoor.doorTrigger.interactable = false;
                twinDoor.doorTrigger.timeToHold = timeToLockPick;
                twinDoor.doorTrigger.timeToHoldSpeedMultiplier = 1.0f;
                twinDoor.doorTrigger.hoverTip = lockPickHoverMessage;
                twinDoor.doorTrigger.holdTip = lockPickUnlockMessage;
                twinDoor.isPickingLock = false;
                twinDoor.isLocked = true;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void UnlockDoor()
        {
            doorLockSFX.Stop();
            doorLockSFX.PlayOneShot(unlockSFX);

            if (navMeshObstacle != null)
            {
                navMeshObstacle.carving = false;
                navMeshObstacle.carveOnlyStationary = false;
            }

            if (!isLocked)
            {
                return;
            }

            doorTrigger.interactable = true;
            doorTrigger.timeToHold = defaultTimeToHold;
            doorTrigger.timeToHoldSpeedMultiplier = 1.0f;
            doorTrigger.hoverTip = doorHoverMessage;
            doorTrigger.holdTip = doorUnlockMessage;
            isPickingLock = false;
            isLocked = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void Update()
        {
            if (isLocked)
            {
                doorTrigger.disabledHoverTip = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer?.itemProperties.itemId == 14
                    ? StartOfRound.Instance.localPlayerUsingController ? doorLockedKeyControllerMessage : doorLockedKeyMessage
                    : doorLockedMessage;

                if (playersPickingDoor > 0)
                {
                    playerPickingLockProgress = Mathf.Clamp(playerPickingLockProgress + (playersPickingDoor * 0.85f * Time.deltaTime), 1.0f, 5.5f);
                }

                doorTrigger.timeToHoldSpeedMultiplier = Mathf.Clamp(playersPickingDoor * 0.85f, 1.0f, 3.5f);

                if (isPickingLock)
                {
                    lockPickTimeLeft -= Time.deltaTime;
                    doorTrigger.disabledHoverTip = string.Format(lockPickUnlockTimerMessage, (int)lockPickTimeLeft);

                    if (IsServer && lockPickTimeLeft < 0.0f)
                    {
                        UnlockDoor();
                        UnlockDoorServerRpc();
                    }
                }
            }
            else
            {
                if (navMeshObstacle != null)
                {
                    navMeshObstacle.carving = false;
                }

                if (hauntedDoor)
                {
                    TryDoorHaunt();
                }

                if (StartedLocked && !WasUnlockedLastFrame && !isLocked)
                {
                    doorTrigger.hoverTip = doorHoverMessage;
                    doorTrigger.holdTip = doorUnlockMessage;

                    WasUnlockedLastFrame = true;
                }
            }
        }
    }
}