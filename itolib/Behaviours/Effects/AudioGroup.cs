using DunGen;
using itolib.Enums;
using itolib.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AudioGroup : MonoBehaviour, IActivationScript
    {
        /// <summary>
        ///     Cached instance of the current <c>AudioGroup</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        private AudioSource[]? sources;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Audio Group")]
        [Tooltip("")]
        [SerializeField] private GameObject[]? objectsToSearch;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool autoInitialize;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the <c>AudioSource</c> search.
        /// </summary>
        [field: Tooltip("Desired activation time for the AudioSource search.")]
        [field: FormerlySerializedAs("activationTime")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.DungeonComplete;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the <c>AudioSource</c> search.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [Space(5.0f)]
        [Header("== DEPRECATED ==")]
        [Tooltip("(Deprecated) Desired activation time for the AudioSource search. Should be ignored.")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.DungeonComplete;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        private AudioGroup()
        {
            ActivationSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (activationTime is not ActivationTime.DungeonComplete)
            {
                ActivationTime = activationTime;
            }

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void FindSourcesInObjects()
        {
            for (int i = 0; i < objectsToSearch?.Length; i++)
            {
                if (objectsToSearch[i] != null)
                {
                    sources = sources?.Length > 0
                        ? [.. sources, .. objectsToSearch[i].GetComponentsInChildren<AudioSource>()]
                        : [.. objectsToSearch[i].GetComponentsInChildren<AudioSource>()];
                }
            }

            if (autoInitialize)
            {
                SwitchAll(AudioAction.Initialize);
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            FindSourcesInObjects();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PlayAll()
        {
            SwitchAll(AudioAction.Play);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PauseAll()
        {
            SwitchAll(AudioAction.Pause);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void UnpauseAll()
        {
            SwitchAll(AudioAction.Unpause);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void StopAll()
        {
            SwitchAll(AudioAction.Stop);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void StopAllIncludingOneShots()
        {
            SwitchAll(AudioAction.StopIncludingOneShots);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="action"></param>
        public void SwitchAll(int action)
        {
            SwitchAll((AudioAction)action);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="action"></param>
        public void SwitchAll(AudioAction action)
        {
            for (int i = 0; i < sources?.Length; i++)
            {
                if (sources[i] != null)
                {
                    AudioSource? source = sources[i];

                    if (source != null)
                    {
                        switch (action)
                        {
                            case AudioAction.Initialize:
                                source.Play();
                                source.Pause();
                                break;
                            case AudioAction.Play:
                                source.Play();
                                break;
                            case AudioAction.Pause:
                                if (!source.isPlaying)
                                {
                                    source.Play();
                                }
                                source.Pause();
                                break;
                            case AudioAction.Unpause:
                                if (!source.isPlaying)
                                {
                                    source.Play();
                                    source.Pause();
                                }
                                source.UnPause();
                                break;
                            case AudioAction.Stop:
                                source.Stop();
                                break;
                            case AudioAction.StopIncludingOneShots:
                                source.Stop(true);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="source"></param>
        public void SyncWith(AudioSource source)
        {
            for (int i = 0; i < sources?.Length; i++)
            {
                sources[i].time = source.time;
            }
        }

        /// <summary>
        ///     <c>DunGen</c> listener called when the Dungeon finishes generating.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            ActivationSelf.OnDungeonComplete();
        }
    }
}