using itolib.Behaviours.Networking;
using itolib.Enums;
using itolib.Util;
using System;
using System.Collections;
using UnityEngine;

namespace itolib.Behaviours.Notifications
{
    /// <summary>
    ///     Represents a single notification entry to display to alerted players.
    /// </summary>
    [Serializable]
    public struct NotificationEntry
    {
        /// <summary>
        ///     Type of notification to display. Affects notification color, animation, and default opening sound effect.
        /// </summary>
        [Header("Notification Entry")]
        [Tooltip("Type of notification to display. Affects notification color, animation, and default opening sound effect.")]
        public MessageType messageType = MessageType.Notification;

        /// <summary>
        ///     Main body of text to display on the notification.
        /// </summary>
        [Tooltip("Main body of text to display on the notification.")]
        [TextArea(3, 10)]
        public string bodyText = string.Empty;

        /// <summary>
        ///     Amount of time the notification should be displayed, in seconds. Default vanilla wait time is around <c>6.8</c> seconds.
        /// </summary>
        [Tooltip("Amount of time the notification should be displayed, in seconds. Default vanilla wait time is around '6.8' seconds.")]
        [Min(0.0f)]
        public float waitTime = 6.25f;

        /// <summary>
        ///     Amount of time between each letter being added onto the notification, in seconds. Can be set to <c>0</c> to have the text display instantly.
        /// </summary>
        [Tooltip("Amount of time between each letter being added onto the notification, in seconds. Can be set to '0' to have the text display instantly.")]
        [Min(0.0f)]
        public float letterDelay;

        /// <summary>
        ///     Whether the notification opening sound effect (which depends on selected <c>NotificationType</c>) should be replaced or not.
        /// </summary>
        [Space(5.0f)]
        [Header(header: "SFX")]
        [Tooltip("Whether the notification opening sound effect (which depends on selected notification type) should be replaced or not.")]
        public bool overrideOpenSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to replace the notification opening sound effect with.
        /// </summary>
        [Tooltip("List of audio clips to replace the notification opening sound effect with.")]
        public AudioClip?[]? notificationOpenSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to use for the notification letter sound effect.
        /// </summary>
        [Tooltip("List of audio clips to use for the notification letter sound effect.")]
        public AudioClip?[]? letterDelaySFX;

        /// <summary>
        ///     Constructor for the struct type (needed to allow default parameter values).
        /// </summary>
        public NotificationEntry() { }
    }

    /// <summary>
    ///     Represents an alert or message sent to the player, in the form of a notification (e.g. new creature data alert).
    /// </summary>
    public class AlertNotification : NetworkedAlert<NotificationEntry>
    {
        /// <summary>
        ///     Whether the notification opening sound effect (which depends on selected <c>NotificationType</c>) should play or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Alert Notification")]
        [Tooltip("Whether the notification opening sound effect (which depends on selected notification type) should play or not.")]
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
        ///     Time to wait before pausing the notification animation, to display it for longer.
        /// </summary>
        private const float notificationAnimationDuration = 6.25f;

        /// <summary>
        ///     <c>Coroutine</c> for the current notification, if any are playing.
        /// </summary>
        private Coroutine? notificationCoroutine;

        /// <summary>
        ///     Cached array for playing a single <c>AudioClip</c>.
        /// </summary>
        private readonly AudioClip[] clipSingle = new AudioClip[1];

        /// <summary>
        ///     Display notification entries, sequentially.
        /// </summary>
        /// <param name="alerts">Notification entries to display.</param>
        protected override void PlayAlerts(NotificationEntry[] alerts)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud != null)
            {
                if (notificationCoroutine != null)
                {
                    // Stop any existing notification Coroutine.
                    hud.StopCoroutine(notificationCoroutine);
                }

                // Start Coroutine to display the notification entries sequentially, starting from the given index.
                notificationCoroutine = hud.StartCoroutine(DisplayNotifications(alerts));
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to display the given notification entries sequentially.
        /// </summary>
        /// <param name="notificationArray">List of notification entries to display.</param>
        private IEnumerator DisplayNotifications(NotificationEntry[] notificationArray)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud == null || notificationArray == null)
            {
                yield break;
            }

            // Play each given notification entry sequentially.
            for (int i = 0; i < notificationArray.Length; i++)
            {
                NotificationEntry notification = notificationArray[i];

                if (hud.globalNotificationAnimator == null)
                {
                    continue;
                }

                // Start counting how long the notification animation has been playing for.
                float animationTime = 0.0f;

                // Set the Trigger parameter for the type of message to display.
                hud.globalNotificationAnimator.SetTrigger(notification.messageType switch
                {
                    MessageType.Tip => triggerHintAnimationID,
                    MessageType.Warning => triggerWarningAnimationID,
                    MessageType.Notification => triggerNotifAnimationID,
                    _ => default,
                });

                if (playOpenSFX && hud.UIAudio != null)
                {
                    if (notification.messageType is MessageType.Notification && hud.globalNotificationSFX != null)
                    {
                        // Obtain vanilla notification sound effect (single audio clip).
                        clipSingle[0] = hud.globalNotificationSFX;
                    }

                    // Obtain list of audio clips to play when opening the notification.
                    AudioClip?[]? openClips = (!notification.overrideOpenSFX || notification.notificationOpenSFX == null || notification.notificationOpenSFX.Length == 0)
                        ? (notification.messageType switch
                        {
                            MessageType.Tip => (hud.tipsSFX?.Length > 0) ? hud.tipsSFX : null,
                            MessageType.Warning => (hud.warningSFX?.Length > 0) ? hud.warningSFX : null,
                            MessageType.Notification => clipSingle,
                            _ => default
                        }) : notification.notificationOpenSFX;

                    if (openClips?.Length > 0)
                    {
                        // Play a random sound effect from the list of audio clips.
                        hud.UIAudio.PlayOneShot(openClips[UnityEngine.Random.Range(0, openClips.Length)]);
                    }
                }

                if (hud.globalNotificationText == null)
                {
                    continue;
                }

                // Check if body text should be displayed with a delay between each letter.
                if (notification.letterDelay > 0.0f)
                {
                    // Clear notification body text.
                    hud.globalNotificationText.text = string.Empty;

                    // Add each letter to the body text, with the specified delay.
                    for (int j = 0; j < notification.bodyText.Length; j++)
                    {
                        // Add letter to the notification.
                        hud.globalNotificationText.text += notification.bodyText[j];

                        if (hud.UIAudio != null)
                        {
                            // Obtain list of audio clips to play when a letter is added.
                            AudioClip?[]? letterClips = (notification.letterDelaySFX?.Length > 0) ? notification.letterDelaySFX : null;

                            if (letterClips?.Length > 0)
                            {
                                // Play a random sound effect from the list of audio clips.
                                hud.UIAudio.PlayOneShot(letterClips[UnityEngine.Random.Range(0, letterClips.Length)]);
                            }
                        }

                        // Pause animation if current time is longer than the notification animation.
                        if (animationTime > notificationAnimationDuration)
                        {
                            hud.globalNotificationAnimator.enabled = false;
                        }

                        // Wait for the specified amount of time before adding the next letter.
                        yield return Yielders.WaitForSeconds(notification.letterDelay);
                        animationTime += notification.letterDelay;
                    }
                }
                else
                {
                    // Set notification body text.
                    hud.globalNotificationText.text = notification.bodyText;
                }

                // TODO: Handle values smaller than 'notificationAnimationDuration'.

                if ((notificationArray[i].waitTime + animationTime) > notificationAnimationDuration)
                {
                    // Pause animation if wait time is longer than the notification animation.
                    if (notificationAnimationDuration - animationTime > 0.0f)
                    {
                        yield return Yielders.WaitForSeconds(notificationAnimationDuration - animationTime);
                    }
                    hud.tipsPanelAnimator.enabled = false;

                    yield return Yielders.WaitForSeconds(notificationArray[i].waitTime - notificationAnimationDuration);
                    hud.tipsPanelAnimator.enabled = true;
                }
                else
                {
                    // Wait for the specified amount of time before closing the notification and moving onto the next (if there is one).
                    yield return Yielders.WaitForSeconds(notificationArray[i].waitTime);
                }
            }
        }
    }
}