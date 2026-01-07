using itolib.Behaviours.Networking;
using itolib.Extensions;
using itolib.Util;
using System;
using System.Collections;
using UnityEngine;

namespace itolib.Behaviours.Notifications
{
    /// <summary>
    ///     Represents a single signal message entry to display to alerted players.
    /// </summary>
    [Serializable]
    public struct SignalEntry
    {
        /// <summary>
        ///     Text to transmit on the signal message.
        /// </summary>
        [Header("Signal Entry")]
        [Tooltip("Text to transmit on the signal message.")]
        [TextArea(2, 5)]
        public string signalText = string.Empty;

        /// <summary>
        ///     Amount of time to wait before beginning to type the signal message, in seconds. Default vanilla delay is <c>1.21</c> seconds.
        /// </summary>
        [Tooltip("Amount of time to wait before beginning to type the signal message, in seconds. Default vanilla delay is '1.21' seconds.")]
        [Min(0.0f)]
        public float openDelay = 1.21f;

        /// <summary>
        ///     Amount of time between each letter being added onto the signal message, in seconds. Default vanilla delay is <c>0.7</c> seconds, with some variation.
        ///     Can be set to <c>0</c> to have the text be fully typed instantly.
        /// </summary>
        [Tooltip("Amount of time between each letter being added onto the signal message, in seconds. Default vanilla delay is '0.7' seconds, with some "
            + "variation. Can be set to '0' to have the text be fully typed instantly.")]
        [Min(0.0f)]
        public float typingDelay = 0.7f;

        /// <summary>
        ///     Amount of time the signal message should remain open after the text is fully shown, in seconds. Default vanilla delay is <c>6.5</c> seconds.
        /// </summary>
        [Tooltip("Amount of time the signal message should remain open after the text is fully shown, in seconds. Default vanilla delay is '6.5' seconds.")]
        [Min(0.0f)]
        public float finishDelay = 6.5f;

        /// <summary>
        ///     Whether the signal message open sound effect should be replaced or not.
        /// </summary>
        [Space(5.0f)]
        [Header(header: "SFX")]
        [Tooltip("Whether the signal message open sound effect should be replaced or not.")]
        public bool overrideOpenSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to play when the signal message is opened.
        /// </summary>
        [Tooltip("List of audio clips to play when the signal message is opened.")]
        public AudioClip?[]? signalOpenSFX;

        /// <summary>
        ///     Whether the signal message typing sound effect should be replaced or not.
        /// </summary>
        [Tooltip("Whether the signal message typing sound effect should be replaced or not.")]
        public bool overrideTypingSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to replace the signal message typing sound effect with.
        /// </summary>
        [Tooltip("List of audio clips to replace the signal message typing sound effect with.")]
        public AudioClip?[]? letterTypingSFX;

        /// <summary>
        ///     Whether the signal message finish sound effect should be replaced or not.
        /// </summary>
        [Tooltip("Whether the signal message finish sound effect should be replaced or not.")]
        public bool overrideFinishSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to play when the signal message finishes being typed.
        /// </summary>
        [Tooltip("List of audio clips to play when the signal message finishes being typed.")]
        public AudioClip?[]? signalFinishSFX;

        /// <summary>
        ///     Constructor for the struct type (needed to allow default parameter values).
        /// </summary>
        public SignalEntry() { }
    }

    /// <summary>
    ///     Represents an alert or message sent to the player, in the form of a signal translator message.
    /// </summary>
    public class AlertSignal : NetworkedAlert<SignalEntry>
    {
        /// <summary>
        ///     <c>ScriptableObject</c> for the signal translator unlockable item.
        /// </summary>
        public static UnlockableItem? SignalTranslatorUnlockableItem
        {
            get
            {
                if (field == null && StartOfRound.Instance != null)
                {
                    field = StartOfRound.Instance.unlockablesList.unlockables.Find(unlockable =>
                        unlockable.unlockableName.CompareOrdinal("Signal translator"));
                }

                return field;
            }
        }

        /// <summary>
        ///     Container for the <c>SignalTranslator</c> script in the signal translator prefab.
        /// </summary>
        public static Transform? SignalTranslatorContainer
        {
            get
            {
                if (field == null && SignalTranslatorUnlockableItem != null && SignalTranslatorUnlockableItem.prefabObject != null)
                {
                    field = SignalTranslatorUnlockableItem.prefabObject.transform.GetChild(2);
                }

                return field;
            }
        }

        /// <summary>
        ///     <c>AudioClip</c> played when a signal translator message starts.
        /// </summary>
        public static AudioClip? StartTransmissionClip
        {
            get
            {
                if (field == null && SignalTranslatorContainer != null && SignalTranslatorContainer.TryGetComponent(out SignalTranslator signal))
                {
                    field = signal.startTransmissionSFX;
                }

                return field;
            }
        }

        /// <summary>
        ///     List of <c>AudioClips</c> played when a signal translator message is being typed.
        /// </summary>
        public static AudioClip[]? TypeTextClips
        {
            get
            {
                if (field == null && SignalTranslatorContainer != null && SignalTranslatorContainer.TryGetComponent(out SignalTranslator signal))
                {
                    field = signal.typeTextClips;
                }

                return field;
            }
        }

        /// <summary>
        ///     <c>AudioClip</c> played when a signal translator message is fully typed.
        /// </summary>
        public static AudioClip? FinishTypingClip
        {
            get
            {
                if (field == null && SignalTranslatorContainer != null && SignalTranslatorContainer.TryGetComponent(out SignalTranslator signal))
                {
                    field = signal.finishTypingSFX;
                }

                return field;
            }
        }

        /// <summary>
        ///     Hash of the bool parameter to toggle to display the signal translator message.
        /// </summary>
        private static readonly int startTransmissionID = Animator.StringToHash("transmitting");

        /// <summary>
        ///     <c>Coroutine</c> for the current signal message, if any are playing.
        /// </summary>
        private Coroutine? signalCoroutine;

        /// <summary>
        ///     Cached array for playing a single <c>AudioClip</c>.
        /// </summary>
        private readonly AudioClip?[] clipSingle = new AudioClip[1];

        /// <summary>
        ///     Whether the signal message open sound effect should play or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Alert Signal")]
        [Tooltip("Whether the signal message open sound effect should play or not.")]
        [SerializeField] private bool playOpenSFX = true;

        /// <summary>
        ///     List of <c>AudioClips</c> to play when the signal message is opened.
        /// </summary>
        [Tooltip("List of audio clips to play when the signal message is opened.")]
        [SerializeField] private AudioClip?[]? signalOpenSFX;

        /// <summary>
        ///     Whether the signal message typing sound effect should play or not.
        /// </summary>
        [Tooltip("Whether the signal message typing sound effect should play or not.")]
        [SerializeField] private bool playTypingSFX = true;

        /// <summary>
        ///     List of <c>AudioClips</c> to replace the signal message typing sound effect with.
        /// </summary>
        [Tooltip("List of audio clips to replace the signal message typing sound effect with.")]
        [SerializeField] private AudioClip?[]? signalTypingSFX;

        /// <summary>
        ///     Whether the signal message finish sound effect should play or not.
        /// </summary>
        [Tooltip("Whether the signal message finish sound effect should play or not.")]
        [SerializeField] private bool playFinishSFX = true;

        /// <summary>
        ///     List of <c>AudioClips</c> to play when the signal message finishes being typed.
        /// </summary>
        [Tooltip("List of audio clips to play when the signal message finishes being typed.")]
        [SerializeField] private AudioClip?[]? signalFinishSFX;

        /// <summary>
        ///     Transmit signal message entries, sequentially.
        /// </summary>
        /// <param name="alerts">Signal message entries to transmit.</param>
        protected override void PlayAlerts(SignalEntry[] alerts)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud != null)
            {
                if (signalCoroutine != null)
                {
                    // Stop any existing signal message Coroutine.
                    hud.StopCoroutine(signalCoroutine);
                }

                // Start Coroutine to transmit the signal messages sequentially, starting from the given index.
                signalCoroutine = hud.StartCoroutine(TransmitSignalMessage(alerts));
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to transmit the given signal message entries sequentially.
        /// </summary>
        /// <param name="signalArray">List of signal message entries to transmit.</param>
        private IEnumerator TransmitSignalMessage(SignalEntry[] signalArray)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud == null || hud.signalTranslatorAnimator == null || signalArray == null)
            {
                yield break;
            }

            // Enable the signal translator bool parameter, to open it.
            hud.signalTranslatorAnimator.SetBool(startTransmissionID, true);

            for (int i = 0; i < signalArray.Length; i++)
            {
                // Obtain signal message entry at the current index.
                SignalEntry signal = signalArray[i];

                if (playOpenSFX && hud.UIAudio != null)
                {
                    // Obtain vanilla transmission start sound effect (single audio clip).
                    clipSingle[0] = StartTransmissionClip;

                    // Obtain list of audio clips to play when opening the signal message.
                    AudioClip?[]? openClips = (signal.overrideOpenSFX && signal.signalOpenSFX?.Length > 0)
                        ? signal.signalOpenSFX : ((signalOpenSFX?.Length > 0)
                        ? signalOpenSFX : clipSingle);

                    if (openClips?.Length > 0)
                    {
                        // Play a random sound effect from the list of audio clips.
                        hud.UIAudio.PlayOneShot(openClips[UnityEngine.Random.Range(0, openClips.Length)]);
                    }
                }

                if (hud.signalTranslatorText == null)
                {
                    continue;
                }

                // Clear signal message body text.
                hud.signalTranslatorText.text = string.Empty;

                if (signal.openDelay > 0.0f)
                {
                    // Wait for the specified amount of time before opening the signal message.
                    yield return Yielders.WaitForSeconds(signal.openDelay);
                }

                // Check if signal message should be transmitted with a delay between each letter.
                if (signal.typingDelay > 0.0f)
                {
                    // Add each letter to the signal message, with the specified delay.
                    for (int j = 0; j < signal.signalText.Length; j++)
                    {
                        // Add letter to the signal message.
                        hud.signalTranslatorText.text += signal.signalText[j];

                        if (hud.UIAudio != null)
                        {
                            // Obtain list of audio clips to play when a letter is added.
                            AudioClip?[]? typingClips = (signal.overrideTypingSFX && signal.letterTypingSFX?.Length > 0)
                                ? signal.letterTypingSFX : (playTypingSFX && signalTypingSFX?.Length > 0)
                                ? signalTypingSFX : ((signal.overrideTypingSFX || playTypingSFX) && TypeTextClips?.Length > 0)
                                ? TypeTextClips : null;

                            if (typingClips?.Length > 0)
                            {
                                // Play a random sound effect from the list of audio clips.
                                hud.UIAudio.PlayOneShot(typingClips[UnityEngine.Random.Range(0, typingClips.Length)]);
                            }
                        }

                        // Wait for the specified amount of time before adding the next letter.
                        yield return Yielders.WaitForSeconds(signal.typingDelay);
                    }
                }
                else
                {
                    // Set signal message text.
                    hud.signalTranslatorText.text = signal.signalText;
                }

                if (playFinishSFX && hud.UIAudio != null)
                {
                    // Obtain vanilla typing finish sound effect (single audio clip).
                    clipSingle[0] = FinishTypingClip;

                    // Obtain list of audio clips to play when finishing the signal message.
                    AudioClip?[]? finishClips = (signal.overrideFinishSFX && signal.signalFinishSFX?.Length > 0)
                        ? signal.signalFinishSFX : ((signalFinishSFX?.Length > 0)
                        ? signalFinishSFX : clipSingle);

                    if (finishClips?.Length > 0)
                    {
                        // Play a random sound effect from the list of audio clips.
                        hud.UIAudio.PlayOneShot(finishClips[UnityEngine.Random.Range(0, finishClips.Length)]);
                    }
                }

                if (signal.finishDelay > 0.0f)
                {
                    // Wait for the specified amount of time before closing the signal message and moving onto the next (if there is one).
                    yield return Yielders.WaitForSeconds(signal.finishDelay);
                }
            }

            // Disable the signal translator bool parameter, to close it.
            hud.signalTranslatorAnimator.SetBool(startTransmissionID, false);
        }
    }
}