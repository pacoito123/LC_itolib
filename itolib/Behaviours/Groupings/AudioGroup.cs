using itolib.Enums;
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
                PerformGroupAction(source =>
                {
                    source.Play();
                    source.Pause();
                });
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PlayAll()
        {
            PerformGroupAction(source => source.Play());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PauseAll()
        {
            PerformGroupAction(source =>
            {
                if (!source.isPlaying)
                {
                    source.Play();
                }

                source.Pause();
            });
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void UnpauseAll()
        {
            PerformGroupAction(source =>
            {
                if (!source.isPlaying)
                {
                    source.Play();
                    source.Pause();
                }

                source.UnPause();
            });
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void StopAll()
        {
            PerformGroupAction(source => source.Stop());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void StopAllIncludingOneShots()
        {
            PerformGroupAction(source => source.Stop(stopOneShots: true));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="syncSource"></param>
        public void SyncWith(AudioSource syncSource)
        {
            PerformGroupAction(source => source.time = syncSource.time);
        }
    }
}