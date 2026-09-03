using itolib.Enums;
using System;
using UnityEngine;

namespace itolib.Behaviours.Groupings
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AudioGroup : ComponentGroup<AudioSource>
    {
        /// <summary>
        ///     TODO.
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
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Audio Group")]
        [Tooltip("")]
        [SerializeField] private bool autoInitialize;

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public override void PerformActivation(ActivationTime activationTime)
        {
            base.PerformActivation(activationTime);

            if (autoInitialize)
            {
                PerformGroupAction(AudioActions.Pause);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="actionID"></param>
        /// <param name="parameter"></param>
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

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="enabled"></param>
        protected override void EnableSingleComponent(AudioSource source, bool enabled)
        {
            source.enabled = enabled;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="source"></param>
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