using itolib.Behaviours.Networking;
using itolib.Enums;
using itolib.Util;
using System;
using System.Collections;
using UnityEngine;

namespace itolib.Behaviours.Notifications
{
    /// <summary>
    ///     Represents a single toast entry to display to alerted players.
    /// </summary>
    [Serializable]
    public struct ToastEntry : IAlertEntry
    {
        /// <summary>
        ///     Type of toast to display. Affects toast color, animation, and default opening sound effect.
        /// </summary>
        [Header("Toast Entry")]
        [Tooltip("Type of toast to display. Affects toast color, animation, and default opening sound effect.")]
        public MessageType messageType = MessageType.Tip;

        /// <summary>
        ///     Header text to display on the toast.
        /// </summary>
        [Tooltip("Header text to display on the toast.")]
        public string headerText = string.Empty;

        /// <summary>
        ///     Main body of text to display on the toast.
        /// </summary>
        [Tooltip("Main body of text to display on the toast.")]
        [TextArea(3, 10)]
        public string bodyText = string.Empty;

        /// <summary>
        ///     Amount of time the toast should be displayed, in seconds. Default vanilla wait time is around <c>6.8</c> seconds.
        /// </summary>
        [Tooltip("Amount of time the toast should be displayed, in seconds. Default vanilla wait time is around '6.8' seconds.")]
        [Min(0.0f)]
        public float waitTime = 6.25f;

        /// <summary>
        ///     Amount of time between each letter being added onto the toast, in seconds. Can be set to <c>0</c> to have the text display instantly.
        /// </summary>
        [Tooltip("Amount of time between each letter being added onto the toast, in seconds. Can be set to '0' to have the text display instantly.")]
        [Min(0.0f)]
        public float letterDelay;

        /// <summary>
        ///     Whether the toast opening sound effect (which depends on selected <c>ToastType</c>) should be replaced or not.
        /// </summary>
        [Space(5.0f)]
        [Header(header: "SFX")]
        [Tooltip("Whether the toast opening sound effect (which depends on selected toast type) should be replaced or not.")]
        public bool overrideOpenSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to replace the toast opening sound effect with.
        /// </summary>
        [Tooltip("List of audio clips to replace the toast opening sound effect with.")]
        public AudioClip?[]? toastOpenSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to use for the toast letter sound effect.
        /// </summary>
        [Tooltip("List of audio clips to use for the toast letter sound effect.")]
        public AudioClip?[]? letterDelaySFX;

        /// <summary>
        ///     Whether this toast entry should only be displayed once or not.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header(header: "Other")]
        [field: Tooltip("Whether this toast entry should only be displayed once or not.")]
        [field: SerializeField] public bool SingleUse { get; set; }

        /// <summary>
        ///     Constructor for the struct type (needed to allow default parameter values).
        /// </summary>
        public ToastEntry() { }
    }

    /// <summary>
    ///     Represents an alert or message sent to the player, in the form of a toast (e.g. dropship items missed alert).
    /// </summary>
    public class AlertToast : NetworkedAlert<ToastEntry>
    {
        /// <summary>
        ///     Whether the toast opening sound effect (which depends on selected <c>ToastType</c>) should play or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Alert Toast")]
        [Tooltip("Whether the toast opening sound effect (which depends on selected toast type) should play or not.")]
        [SerializeField] private bool playOpenSFX = true;

        /// <summary>
        ///     Hash of the parameter to trigger a hint.
        /// </summary>
        private static readonly int triggerHintAnimationID = Animator.StringToHash("TriggerHint");

        /// <summary>
        ///     Hash of the parameter to trigger a warning.
        /// </summary>
        private static readonly int triggerWarningAnimationID = Animator.StringToHash("TriggerWarning");

        /// <summary>
        ///     Hash of the parameter to trigger a notification.
        /// </summary>
        private static readonly int triggerNotifAnimationID = Animator.StringToHash("TriggerNotif");

        /// <summary>
        ///     Time to wait before pausing the toast animation, to display it for longer.
        /// </summary>
        private const float toastAnimationDuration = 6.25f;

        /// <summary>
        ///     Cached array for playing a single <c>AudioClip</c>.
        /// </summary>
        private readonly AudioClip[] clipSingle = new AudioClip[1];

        /// <summary>
        ///     Display toast entries, sequentially.
        /// </summary>
        /// <param name="alerts">Toast entries to display.</param>
        /// <param name="startingIndex">Index to skip to when displaying the toast entries.</param>
        protected override void PlayAlerts(ToastEntry[] alerts, int startingIndex)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud != null)
            {
                if (hud.tipsPanelCoroutine != null)
                {
                    // Stop any existing toast Coroutine.
                    hud.StopCoroutine(hud.tipsPanelCoroutine);
                }

                // Start Coroutine to display the toast entries sequentially, starting from the given index.
                hud.tipsPanelCoroutine = hud.StartCoroutine(DisplayToasts(alerts, startingIndex));
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to display the given toast entries sequentially.
        /// </summary>
        /// <param name="toastArray">List of toast entries to display.</param>
        /// <param name="startingIndex">Index to skip to when displaying the toast entries.</param>
        private IEnumerator DisplayToasts(ToastEntry[] toastArray, int startingIndex)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud == null || toastArray == null || hud.tipsPanelAnimator == null || hud.tipsPanelBody == null)
            {
                yield break;
            }

            // Play each given toast entry sequentially.
            for (int i = startingIndex; i < toastArray.Length; i++)
            {
                // Obtain toast entry at the current index.
                ToastEntry toast = toastArray[i];

                // Check if toast entry is exhausted.
                if (CheckSingleUse(toast, i))
                {
                    continue;
                }

                // Start counting how long the toast animation has been playing for.
                float animationTime = 0.0f;

                // Obtain hash of the trigger parameter for the type of message to display.
                int messageType = toast.messageType switch
                {
                    MessageType.Tip => triggerHintAnimationID,
                    MessageType.Warning => triggerWarningAnimationID,
                    MessageType.Notification => triggerNotifAnimationID,
                    _ => default,
                };

                // Set trigger parameter for the type of message to display.
                hud.tipsPanelAnimator.SetTrigger(messageType);

                if (hud.tipsPanelHeader != null)
                {
                    // Set toast header text.
                    hud.tipsPanelHeader.text = toast.headerText;
                }

                if (playOpenSFX && hud.UIAudio != null)
                {
                    if (toast.messageType is MessageType.Notification && hud.globalNotificationSFX != null)
                    {
                        // Obtain vanilla notification sound effect (single audio clip).
                        clipSingle[0] = hud.globalNotificationSFX;
                    }

                    // Obtain list of audio clips to play when opening the toast.
                    AudioClip?[]? openClips = (!toast.overrideOpenSFX || toast.toastOpenSFX == null || toast.toastOpenSFX.Length == 0)
                        ? (toast.messageType switch
                        {
                            MessageType.Tip => (hud.tipsSFX?.Length > 0) ? hud.tipsSFX : null,
                            MessageType.Warning => (hud.warningSFX?.Length > 0) ? hud.warningSFX : null,
                            MessageType.Notification => clipSingle,
                            _ => default
                        }) : toast.toastOpenSFX;

                    if (openClips?.Length > 0)
                    {
                        // Play a random sound effect from the list of audio clips.
                        hud.UIAudio.PlayOneShot(openClips[UnityEngine.Random.Range(0, openClips.Length)]);
                    }
                }

                // Check if body text should be displayed with a delay between each letter.
                if (toast.letterDelay > 0.0f)
                {
                    // Clear toast body text.
                    hud.tipsPanelBody.text = string.Empty;

                    // Add each letter to the body text, with the specified delay.
                    for (int j = 0; j < toast.bodyText.Length; j++)
                    {
                        // Add letter to the toast.
                        hud.tipsPanelBody.text += toast.bodyText[j];

                        if (hud.UIAudio != null)
                        {
                            // Obtain list of audio clips to play when a letter is added.
                            AudioClip?[]? letterClips = (toast.letterDelaySFX?.Length > 0) ? toast.letterDelaySFX : null;

                            if (letterClips?.Length > 0)
                            {
                                // Play a random sound effect from the list of audio clips.
                                hud.UIAudio.PlayOneShot(letterClips[UnityEngine.Random.Range(0, letterClips.Length)]);
                            }
                        }

                        // Pause animation if current time is longer than the toast animation.
                        if (animationTime > toastAnimationDuration)
                        {
                            hud.tipsPanelAnimator.enabled = false;
                        }

                        // Wait for the specified amount of time before adding the next letter.
                        yield return Yielders.WaitForSeconds(toast.letterDelay);
                        animationTime += toast.letterDelay;
                    }
                }
                else
                {
                    // Set toast body text.
                    hud.tipsPanelBody.text = toast.bodyText;
                }

                // TODO: Handle values smaller than 'toastAnimationDuration'.

                if ((toast.waitTime + animationTime) > toastAnimationDuration)
                {
                    // Pause animation if wait time is longer than the toast animation.
                    if (toastAnimationDuration - animationTime > 0.0f)
                    {
                        yield return Yielders.WaitForSeconds(toastAnimationDuration - animationTime);
                    }
                    hud.tipsPanelAnimator.enabled = false;

                    yield return Yielders.WaitForSeconds(toast.waitTime - toastAnimationDuration);
                    hud.tipsPanelAnimator.enabled = true;
                }
                else
                {
                    // Wait for the specified amount of time before closing the toast and moving onto the next (if there is one).
                    yield return Yielders.WaitForSeconds(toast.waitTime);
                }

                // Reset trigger parameter after displaying the alert.
                hud.tipsPanelAnimator.ResetTrigger(messageType);
            }
        }
    }
}