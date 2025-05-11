using GameNetcodeStuff;
using UnityEngine;

namespace itolib.Behaviours.Interactables
{
    /// <summary>
    ///     Represents a ladder with customizable climbing speed.
    /// </summary>
    public class InteractClimbable : InteractTrigger
    {
        /// <summary>
        ///     The climbing speed to set when using this specific ladder.
        /// </summary>
        [Header("Ladder")]
        [Tooltip("The climbing speed to set when using this specific ladder.")]
        public float climbSpeed = 15.0f;

        /// <summary>
        ///     The player's regular climbing speed, to reset their climbing speed after they get off the ladder.
        /// </summary>
        [HideInInspector]
        private float normalClimbSpeed = 3.0f;

        /// <summary>
        ///     Set default ladder properties.
        /// </summary>
        /// <remarks>NOTE: Still need to set the Transform points for the InteractTrigger, but the settings here are in default vanilla ladders.</remarks>
        private void Reset()
        {
            hoverTip = "Climb : [LMB]";
            animationWaitTime = 0.5f;
            animationString = "SA_PullLever";
            lockPlayerPosition = true;
            isLadder = true;
        }

        private void Awake()
        {
            // Obtain and save normal player climbing speed.
            normalClimbSpeed = GameNetworkManager.Instance.localPlayerController.climbSpeed;

            // Add call to set player climbing speed to the ladder's specified value, upon attaching.
            onInteractEarly.AddListener(_ => GameNetworkManager.Instance.localPlayerController.climbSpeed = climbSpeed);

            void resetClimbSpeed(PlayerControllerB _)
            {
                // Reset player climbing speed back to previous amount.
                GameNetworkManager.Instance.localPlayerController.climbSpeed = normalClimbSpeed;
            }

            // Add calls to reset player climbing speed upon getting off the ladder.
            onInteract.AddListener(resetClimbSpeed);
            onCancelAnimation.AddListener(resetClimbSpeed);
        }
    }
}