using itolib.Enums;
using System;
using UnityEngine;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     Represents a group of <c>AudioSource</c> components to perform bulk operations on.
    /// </summary>
    public class AudioGroup : ComponentGroup<AudioSource>
    {
        /// <summary>
        ///     All actions able to be performed on every <c>AudioSource</c> component in the group.
        /// </summary>
        private enum AudioActions : byte
        {
            Play,
            Pause,
            Unpause,
            Stop,
            StopIncludingOneShots,
            SyncWith
        }

        /// <summary>
        ///     Whether to automatically initialize (play then immediately pause) every <c>AudioSource</c> component in the group or not.
        /// </summary>
        [Space(5.0f)]
        [Header("Audio Group")]
        [Tooltip("Whether to automatically initialize (play then immediately pause) every AudioSource component in the group or not.")]
        [SerializeField] private bool autoInitialize;

        /// <inheritdoc/>
        public override void PerformActivation(ActivationTime activationTime)
        {
            base.PerformActivation(activationTime);

            if (autoInitialize)
            {
                PerformGroupAction(AudioActions.Pause);
            }
        }

        /// <inheritdoc/>
        protected override void PerformSingleAction(AudioSource source, Enum actionID, object? parameter = null)
        {
            if (actionID is not AudioActions audioActionID)
            {
                return;
            }

            switch (audioActionID)
            {
                case AudioActions.Play:
                    source.Play();
                    break;
                case AudioActions.Pause:
                    if (!source.isPlaying)
                    {
                        source.Play();
                    }
                    source.Pause();
                    break;
                case AudioActions.Unpause:
                    if (!source.isPlaying)
                    {
                        source.Play();
                        source.Pause();
                    }
                    source.UnPause();
                    break;
                case AudioActions.Stop:
                    source.Stop();
                    break;
                case AudioActions.StopIncludingOneShots:
                    source.Stop(stopOneShots: true);
                    break;
                case AudioActions.SyncWith:
                    if (parameter is AudioSource syncSource)
                    {
                        source.time = syncSource.time;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void EnableSingleComponent(AudioSource source, bool enabled)
        {
            source.enabled = enabled;
        }

        /// <inheritdoc/>
        protected override void ToggleSingleComponent(AudioSource source)
        {
            source.enabled = !source.enabled;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PlayAll()
        {
            PerformGroupAction(AudioActions.Play);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PauseAll()
        {
            PerformGroupAction(AudioActions.Pause);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void UnpauseAll()
        {
            PerformGroupAction(AudioActions.Unpause);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void StopAll()
        {
            PerformGroupAction(AudioActions.Stop);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void StopAllIncludingOneShots()
        {
            PerformGroupAction(AudioActions.StopIncludingOneShots);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="syncSource"></param>
        public void SyncWith(AudioSource syncSource)
        {
            PerformGroupAction(AudioActions.SyncWith, syncSource);
        }
    }
}