using System;
using System.Collections;
using itolib.Util;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Notifications
{
    /// <summary>
    ///     Represents a single dialogue entry to display to alerted players.
    /// </summary>
    [Serializable]
    public struct DialogueEntry
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
        ///     Amount of time the dialogue box should remain open after the text is fully shown, in seconds.
        /// </summary>
        [Tooltip("Amount of time the dialogue box should remain open after the text is fully shown, in seconds.")]
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
        ///     Constructor for the struct type (needed to allow default parameter values).
        /// </summary>
        public DialogueEntry() { }
    }

    /// <summary>
    ///     Represents an alert or message sent to the player, in the form of a dialogue box (e.g. ship leaving alert).
    /// </summary>
    public class AlertDialogue : NetworkBehaviour
    {
        /// <summary>
        ///     Hash of the bool parameter to toggle to display the dialogue box.
        /// </summary>
        private static readonly int openAnimationID = Animator.StringToHash("Open");

        /// <summary>
        ///     List of every <c>DialogueEntry</c> to display to alerted players, either sequentially or individually.
        /// </summary>
        [Header("Alert Dialogue")]
        [Tooltip("List of every dialogue entry to display to alerted players, either sequentially or individually.")]
        [SerializeField] private DialogueEntry[]? dialogueEntries;

        /// <summary>
        ///     Whether the dialogue box opening sound effect should play or not.
        /// </summary>
        [Space(5.0f)]
        [Header("SFX")]
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
        ///     Display each dialogue entry sequentially, starting from the beginning.
        /// </summary>
        public void PlayDialogueSequence()
        {
            PlayDialogueSequence(0);
        }

        /// <summary>
        ///     Display each dialogue entry sequentially, starting from a specific point.
        /// </summary>
        /// <param name="startingIndex">Index to skip to when displaying the dialogue entries.</param>
        public void PlayDialogueSequence(int startingIndex)
        {
            // Check if given index is valid.
            if (dialogueEntries?.Length > startingIndex)
            {
                // Display dialogue sequence for the local client.
                PlayDialogueSequenceLocal(startingIndex);

                // Check if object is spawned.
                if (IsSpawned)
                {
                    // Display dialogue sequence for all other clients.
                    PlayDialogueSequenceRpc(startingIndex);
                }
            }
        }

        /// <summary>
        ///     Display dialogue sequence for all other clients.
        /// </summary>
        /// <param name="startingIndex"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlayDialogueSequenceRpc(int startingIndex)
        {
            // Display dialogue sequence for the local client.
            PlayDialogueSequenceLocal(startingIndex);
        }

        /// <summary>
        ///     Display each dialogue entry sequentially for the local client, starting from the beginning.
        /// </summary>
        public void PlayDialogueSequenceLocal()
        {
            PlayDialogueSequenceLocal(0);
        }

        /// <summary>
        ///     Display each dialogue entry sequentially for the local client, starting from a specific point.
        /// </summary>
        /// <param name="startingIndex">Index to skip to when playing the dialogue entries.</param>
        public void PlayDialogueSequenceLocal(int startingIndex)
        {
            // Check if given index is valid.
            if (dialogueEntries?.Length > startingIndex)
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
                    hud.readDialogueCoroutine = hud.StartCoroutine(ReadOutDialogue(dialogueEntries[startingIndex..]));
                }
            }
        }

        /// <summary>
        ///     Display a single dialogue entry.
        /// </summary>
        /// <param name="dialogueIndex">Index of the dialogue entry to play.</param>
        public void PlayDialogueSingle(int dialogueIndex)
        {
            // Check if given index is valid.
            if (dialogueEntries?.Length > dialogueIndex)
            {
                // Display a single dialogue entry for the local client.
                PlayDialogueSingleLocal(dialogueIndex);

                if (IsSpawned)
                {
                    // Display a single dialogue entry for all other clients.
                    PlayDialogueSingleRpc(dialogueIndex);
                }
            }
        }

        /// <summary>
        ///     Display a single dialogue entry for all other clients.
        /// </summary>
        /// <param name="dialogueIndex">Index of the dialogue entry to play.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlayDialogueSingleRpc(int dialogueIndex)
        {
            // Display a single dialogue entry for the local client.
            PlayDialogueSingleLocal(dialogueIndex);
        }

        /// <summary>
        ///     Display a single dialogue entry for the local client.
        /// </summary>
        /// <param name="dialogueIndex">Index of the dialogue entry to play.</param>
        public void PlayDialogueSingleLocal(int dialogueIndex)
        {
            // Check if given index is valid.
            if (dialogueEntries?.Length > dialogueIndex)
            {
                HUDManager? hud = HUDManager.Instance;

                if (hud != null)
                {
                    if (hud.readDialogueCoroutine != null)
                    {
                        // Stop any existing dialogue Coroutine.
                        hud.StopCoroutine(hud.readDialogueCoroutine);
                    }

                    // Start Coroutine to display the single dialogue entry.
                    hud.readDialogueCoroutine = hud.StartCoroutine(ReadOutDialogue([dialogueEntries[dialogueIndex]]));
                }
            }
        }

        /// <summary>
        ///     <c>Coroutine</c> to display the given dialogue entries sequentially.
        /// </summary>
        /// <param name="dialogueArray">List of dialogue entries to display.</param>
        private IEnumerator ReadOutDialogue(DialogueEntry[] dialogueArray)
        {
            HUDManager? hud = HUDManager.Instance;

            if (hud == null || hud.dialogueBoxAnimator == null || dialogueArray == null)
            {
                yield break;
            }

            // Enable the dialogue box bool parameter, to open it.
            hud.dialogueBoxAnimator.SetBool(openAnimationID, true);

            // Play each given dialogue entry sequentially.
            for (int i = 0; i < dialogueArray.Length; i++)
            {
                // Obtain dialogue entry at the current index.
                DialogueEntry dialogue = dialogueArray[i];

                if (hud.dialogeBoxHeaderText != null)
                {
                    // Set dialogue box header text.
                    hud.dialogeBoxHeaderText.text = dialogue.speakerText;
                }

                if (hud.dialogueBoxSFX != null)
                {
                    // Obtain list of audio clips to play when opening the dialogue box.
                    AudioClip?[]? openClips = (dialogue.overrideOpenSFX && dialogue.dialogueOpenSFX?.Length > 0)
                        ? dialogue.dialogueOpenSFX : (playOpenSFX && dialogueOpenSFX?.Length > 0)
                        ? dialogueOpenSFX : ((dialogue.overrideOpenSFX || playOpenSFX) && hud.dialogueBleeps?.Length > 0)
                        ? hud.dialogueBleeps : null;

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

                // Wait for the specified amount of time before closing the dialogue box and moving onto the next (if there is one).
                yield return Yielders.WaitForSeconds(dialogueArray[i].waitTime);
            }

            // Disable the dialogue box bool parameter, to close it.
            hud.dialogueBoxAnimator.SetBool(openAnimationID, false);
        }
    }
}