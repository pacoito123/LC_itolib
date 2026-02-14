using itolib.Behaviours.Networking;
using itolib.Util;
using System;
using System.Collections;
using UnityEngine;

namespace itolib.Behaviours.Notifications
{
    /// <summary>
    ///     Represents a single dialogue entry to display to alerted players.
    /// </summary>
    [Serializable]
    public struct DialogueEntry : IAlertEntry
    {
        /// <summary>
        ///     Header text to display on the dialogue box.
        /// </summary>
        [Header("Dialogue Entry")]
        [Tooltip("Header text to display on the dialogue box.")]
        public string speakerText = string.Empty;

        /// <summary>
        ///     Main body of text to display on the dialogue box.
        /// </summary>
        [Tooltip("Main body of text to display on the dialogue box.")]
        [TextArea(5, 20)]
        public string bodyText = string.Empty;

        /// <summary>
        ///     Amount of time the dialogue box should remain open after the text is fully shown, in seconds. Default vanilla wait time is <c>4</c> seconds.
        /// </summary>
        [Tooltip("Amount of time the dialogue box should remain open after the text is fully shown, in seconds. Default vanilla wait time is '4' seconds.")]
        [Min(0.0f)]
        public float waitTime = 4.0f;

        /// <summary>
        ///     Amount of time between each letter being added onto the dialogue box, in seconds. Can be set to <c>0</c> to have the text display instantly.
        /// </summary>
        [Tooltip("Amount of time between each letter being displayed onto the dialogue box, in seconds. Can be set to '0' to have the text display instantly.")]
        [Min(0.0f)]
        public float typingDelay;

        /// <summary>
        ///     Whether the dialogue box opening sound effect should be replaced or not.
        /// </summary>
        [Space(5.0f)]
        [Header(header: "SFX")]
        [Tooltip("Whether the dialogue box opening sound effect should be replaced or not.")]
        public bool overrideOpenSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to replace the dialogue box opening sound effect with.
        /// </summary>
        [Tooltip("List of audio clips to replace the dialogue box opening sound effect with.")]
        public AudioClip?[]? dialogueOpenSFX;

        /// <summary>
        ///     Whether the dialogue box typing sound effect should be replaced or not.
        /// </summary>
        [Tooltip("Whether the dialogue box typing sound effect should be replaced or not.")]
        public bool overrideTypingSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to replace the dialogue box typing sound effect with.
        /// </summary>
        [Tooltip("List of audio clips to replace the dialogue box typing sound effect with.")]
        public AudioClip?[]? letterTypingSFX;

        /// <summary>
        ///     Whether this dialogue box entry should only be displayed once or not.
        /// </summary>
        [field: Space(5.0f)]
        [field: Header(header: "Other")]
        [field: Tooltip("Whether this dialogue box entry should only be displayed once or not.")]
        [field: SerializeField] public bool SingleUse { get; set; }

        /// <summary>
        ///     Constructor for the struct type (needed to allow default parameter values).
        /// </summary>
        public DialogueEntry() { }
    }

    /// <summary>
    ///     Represents an alert or message sent to the player, in the form of a dialogue box (e.g. ship leaving alert).
    /// </summary>
    public class AlertDialogue : NetworkedAlert<DialogueEntry>
    {
        /// <summary>
        ///     Hash of the bool parameter to toggle to display the dialogue box.
        /// </summary>
        private static readonly int openAnimationID = Animator.StringToHash("Open");

        /// <summary>
        ///     Whether the dialogue box opening sound effect should play or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Alert Dialogue")]
        [Tooltip("Whether the dialogue box opening sound effect should play or not.")]
        [SerializeField] private bool playOpenSFX = true;

        /// <summary>
        ///     List of <c>AudioClips</c> to play when the dialogue box is opened.
        /// </summary>
        [Tooltip("List of audio clips to play when the dialogue box is opened.")]
        [SerializeField] private AudioClip?[]? dialogueOpenSFX;

        /// <summary>
        ///     Whether the dialogue box typing sound effect should play or not.
        /// </summary>
        [Tooltip("Whether the dialogue box typing sound effect should play or not.")]
        [SerializeField] private bool playTypingSFX;

        /// <summary>
        ///     List of <c>AudioClips</c> to play when a letter is added onto the dialogue box.
        /// </summary>
        [Tooltip("List of audio clips to play when a letter is added onto the dialogue box.")]
        [SerializeField] private AudioClip?[]? letterTypingSFX;

        /// <summary>
        ///     List of every <c>DialogueEntry</c> to display to alerted players, either sequentially or individually.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(10.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("[Deprecated] List of every dialogue entry to display to alerted players, either sequentially or individually.")]
        [SerializeField] private DialogueEntry[]? dialogueEntries;

        /// <summary>
        ///     Use deprecated entries, if any are present without any entries in the other list.
        /// </summary>
        protected override void Start()
        {
            if ((alertEntries == null || alertEntries.Length == 0) && dialogueEntries?.Length > 0)
            {
                alertEntries = dialogueEntries;
            }

            base.Start();
        }

        /// <summary>
        ///     Display dialogue entries, sequentially.
        /// </summary>
        /// <param name="alerts">Dialogue entries to display.</param>
        /// <param name="startingIndex">Index to skip to when displaying the dialogue entries.</param>
        protected override void PlayAlerts(DialogueEntry[] alerts, int startingIndex)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud != null)
            {
                if (hud.readDialogueCoroutine != null)
                {
                    // Stop any existing dialogue Coroutine.
                    hud.StopCoroutine(hud.readDialogueCoroutine);
                }

                // Start Coroutine to display the dialogue entries sequentially, starting from the given index.
                hud.readDialogueCoroutine = hud.StartCoroutine(ReadOutDialogue(alerts, startingIndex));
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to display the given dialogue entries sequentially.
        /// </summary>
        /// <param name="dialogueArray">List of dialogue entries to display.</param>
        /// <param name="startingIndex">Index to skip to when displaying the dialogue entries.</param>
        private IEnumerator ReadOutDialogue(DialogueEntry[] dialogueArray, int startingIndex)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud == null || hud.dialogueBoxAnimator == null || hud.dialogeBoxText == null)
            {
                yield break;
            }

            // Enable the dialogue box bool parameter, to open it.
            hud.dialogueBoxAnimator.SetBool(openAnimationID, true);

            // Play each given dialogue entry sequentially.
            for (int i = startingIndex; i < dialogueArray?.Length; i++)
            {
                // Obtain dialogue entry at the current index.
                DialogueEntry dialogue = dialogueArray[i];

                // Check if dialogue entry is exhausted.
                if (CheckSingleUse(dialogue, i))
                {
                    continue;
                }

                if (hud.dialogeBoxHeaderText != null)
                {
                    // Set dialogue box header text.
                    hud.dialogeBoxHeaderText.text = dialogue.speakerText;
                }

                if (hud.dialogueBoxSFX != null)
                {
                    // Obtain list of audio clips to play when opening the dialogue box.
                    AudioClip?[]? openClips = (dialogue.overrideOpenSFX && dialogue.dialogueOpenSFX?.Length > 0)
                        ? dialogue.dialogueOpenSFX : ((playOpenSFX && dialogueOpenSFX?.Length > 0)
                        ? dialogueOpenSFX : (((dialogue.overrideOpenSFX || playOpenSFX) && hud.dialogueBleeps?.Length > 0)
                        ? hud.dialogueBleeps : null));

                    if (openClips?.Length > 0)
                    {
                        // Play a random sound effect from the list of audio clips.
                        hud.dialogueBoxSFX.PlayOneShot(openClips[UnityEngine.Random.Range(0, openClips.Length)]);
                    }
                }

                // Check if body text should be displayed with a delay between each letter.
                if (dialogue.typingDelay > 0.0f)
                {
                    // Clear dialogue box body text.
                    hud.dialogeBoxText.text = string.Empty;

                    // Add each letter to the body text, with the specified delay.
                    for (int j = 0; j < dialogue.bodyText.Length; j++)
                    {
                        // Add letter to the dialogue box.
                        hud.dialogeBoxText.text += dialogue.bodyText[j];

                        if (hud.UIAudio != null)
                        {
                            // Obtain list of audio clips to play when a letter is added.
                            AudioClip?[]? typingClips = (dialogue.overrideTypingSFX && dialogue.letterTypingSFX?.Length > 0)
                                ? dialogue.letterTypingSFX : (playTypingSFX && letterTypingSFX?.Length > 0)
                                ? letterTypingSFX : ((dialogue.overrideTypingSFX || playTypingSFX) && AlertSignal.TypeTextClips?.Length > 0)
                                ? AlertSignal.TypeTextClips : null;

                            if (typingClips?.Length > 0)
                            {
                                // Play a random sound effect from the list of audio clips.
                                hud.UIAudio.PlayOneShot(typingClips[UnityEngine.Random.Range(0, typingClips.Length)]);
                            }
                        }

                        // Wait for the specified amount of time before adding the next letter.
                        yield return Yielders.WaitForSeconds(dialogue.typingDelay);
                    }
                }
                else
                {
                    // Set dialogue box body text.
                    hud.dialogeBoxText.text = dialogue.bodyText;
                }

                if (dialogue.waitTime > 0.0f)
                {
                    // Wait for the specified amount of time before closing the dialogue box and moving onto the next (if there is one).
                    yield return Yielders.WaitForSeconds(dialogue.waitTime);
                }
            }

            // Disable the dialogue box bool parameter, to close it.
            hud.dialogueBoxAnimator.SetBool(openAnimationID, false);
        }

        /// <summary>
        ///     [Deprecated] Display each dialogue entry sequentially, starting from the beginning.
        /// </summary>
        [Obsolete("Switch to PlayAlertSequence() function.")]
        public void PlayDialogueSequence()
        {
            PlayDialogueSequence(0);
        }

        /// <summary>
        ///     [Deprecated] Display each dialogue entry sequentially, starting from a specific point.
        /// </summary>
        /// <param name="startingIndex">Index to skip to when displaying the dialogue entries.</param>
        [Obsolete("Switch to PlayAlertSequence() function.")]
        public void PlayDialogueSequence(int startingIndex)
        {
            PlayAlertSequence(startingIndex);
        }

        /// <summary>
        ///     [Deprecated] Display each dialogue entry sequentially for the local client, starting from the beginning.
        /// </summary>
        [Obsolete("Switch to PlayAlertSequenceLocal() function.")]
        public void PlayDialogueSequenceLocal()
        {
            PlayDialogueSequenceLocal(0);
        }

        /// <summary>
        ///     [Deprecated] Display each dialogue entry sequentially for the local client, starting from a specific point.
        /// </summary>
        /// <param name="startingIndex">Index to skip to when playing the dialogue entries.</param>
        [Obsolete("Switch to PlayAlertSequenceLocal() function.")]
        public void PlayDialogueSequenceLocal(int startingIndex)
        {
            PlayAlertSequenceLocal(startingIndex);
        }

        /// <summary>
        ///     [Deprecated] Display a single dialogue entry.
        /// </summary>
        /// <param name="dialogueIndex">Index of the dialogue entry to play.</param>
        [Obsolete("Switch to PlayAlertSingle() function.")]
        public void PlayDialogueSingle(int dialogueIndex)
        {
            PlayAlertSingle(dialogueIndex);
        }

        /// <summary>
        ///     [Deprecated] Display a single dialogue entry for the local client.
        /// </summary>
        /// <param name="dialogueIndex">Index of the dialogue entry to play.</param>
        [Obsolete("Switch to PlayAlertSingleLocal() function.")]
        public void PlayDialogueSingleLocal(int dialogueIndex)
        {
            PlayAlertSingleLocal(dialogueIndex);
        }
    }
}