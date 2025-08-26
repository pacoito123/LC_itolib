using DunGen;
using itolib.Enums;
using LethalLevelLoader;
using UnityEngine;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AudioGroup : MonoBehaviour, IDungeonCompleteReceiver
    {
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
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.DungeonComplete;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Start()
        {
            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(FindSourcesInObjects);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(FindSourcesInObjects);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.AddListener(FindSourcesInObjects);
                    }
                    break;
                case ActivationTime.Immediate:
                    FindSourcesInObjects();
                    break;
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnDestroy()
        {
            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(FindSourcesInObjects);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(FindSourcesInObjects);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.RemoveListener(FindSourcesInObjects);
                    }
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
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
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (activationTime is ActivationTime.DungeonComplete)
            {
                FindSourcesInObjects();
            }
        }
    }
}