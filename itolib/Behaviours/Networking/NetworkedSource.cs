using System;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct SyncedAudioProperties() : INetworkSerializable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Synced Audio Properties")]
        [Tooltip("")]
        public float volume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float pitch = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float loudness = 0.5f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float walkieVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref volume);
            serializer.SerializeValue(ref pitch);
            serializer.SerializeValue(ref loudness);
            serializer.SerializeValue(ref walkieVolume);
        }
    }

    /// <summary>
    ///     Synced networking for AudioSource components, also contains some additional stuff.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class NetworkedSource : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Networked Source")]
        [Tooltip("")]
        [SerializeField] private AudioSource? syncedSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioClip[]? audioClips;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(3.0f)]
        [Header("Audio Properties")]
        [Tooltip("")]
        [Range(0.0f, 4.0f)]
        [SerializeField] private float minVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 4.0f)]
        [SerializeField] private float maxVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 4.0f)]
        [SerializeField] private float minPitch = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 4.0f)]
        [SerializeField] private float maxPitch = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(3.0f)]
        [Header("Audible Properties")]
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] private float audibleRange = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float minLoudness = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float maxLoudness = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool volumeIsLoudness = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(3.0f)]
        [Header("Walkie Properties")]
        [Tooltip("")]
        [SerializeField] private bool transmitOverWalkie = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool overrideWalkieVolume = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float minWalkieVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float maxWalkieVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Reset()
        {
            syncedSource = GetComponent<AudioSource>();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost && syncedSource != null && syncedSource.playOnAwake)
            {
                PlayAudio();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PlayAudio()
        {
            if (syncedSource != null && syncedSource.clip != null)
            {
                SyncedAudioProperties audioProperties = RollRandomizedProperties();

                PlayAudioLocal(audioProperties);

                if (IsSpawned)
                {
                    PlayAudioRpc(audioProperties);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="audioProperties"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlayAudioRpc(SyncedAudioProperties audioProperties)
        {
            PlayAudioLocal(audioProperties);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PlayAudioLocal()
        {
            PlayAudioLocal(RollRandomizedProperties());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="audioProperties"></param>
        private void PlayAudioLocal(SyncedAudioProperties audioProperties)
        {
            if (syncedSource != null && syncedSource.clip != null)
            {
                float originalVolume = syncedSource.volume, originalPitch = syncedSource.pitch;

                syncedSource.volume = Mathf.Clamp(syncedSource.volume * audioProperties.volume, 0.0f, 1.0f);
                syncedSource.pitch = Mathf.Clamp(syncedSource.pitch * audioProperties.pitch, 0.0f, 1.0f);
                syncedSource.Play();

                if (audibleRange > 0.0f)
                {
                    RoundManager.Instance.PlayAudibleNoise(transform.position, audibleRange, !volumeIsLoudness
                        ? audioProperties.loudness : syncedSource.volume, 0,
                        // item.isInElevator && StartOfRound.Instance.hangarDoorsClosed, 0);
                        false, 0); // TODO: Check ship bounds for inside closed ship.

                    /* if (loudness >= 0.6f && item.playerHeldBy != null)
                    {
                        item.playerHeldBy.timeSinceMakingLoudNoise = 0.0f;
                    } */
                }

                if (transmitOverWalkie)
                {
                    WalkieTalkie.TransmitOneShotAudio(syncedSource, syncedSource.clip, overrideWalkieVolume
                        ? audioProperties.walkieVolume : syncedSource.volume);
                }

                syncedSource.volume = originalVolume;
                syncedSource.pitch = originalPitch;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="clip"></param>
        public void PlayOneShot(int clip)
        {
            if (syncedSource != null && audioClips?.Length > clip && audioClips[clip] != null)
            {
                SyncedAudioProperties audioProperties = RollRandomizedProperties();

                PlayOneShotLocal(clip, audioProperties);

                if (IsSpawned)
                {
                    PlayOneShotRpc(clip, audioProperties);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="audioProperties"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlayOneShotRpc(int clip, SyncedAudioProperties audioProperties)
        {
            PlayOneShotLocal(clip, audioProperties);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="clip"></param>
        public void PlayOneShotLocal(int clip)
        {
            if (syncedSource != null && audioClips?.Length > clip && audioClips[clip] != null)
            {
                PlayOneShotLocal(clip, RollRandomizedProperties());
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="audioProperties"></param>
        private void PlayOneShotLocal(int clip, SyncedAudioProperties audioProperties)
        {
            if (syncedSource != null && audioClips?.Length > clip && audioClips[clip] != null)
            {
                float originalVolume = syncedSource.volume, originalPitch = syncedSource.pitch;

                syncedSource.volume = Mathf.Clamp(syncedSource.volume * audioProperties.volume, 0.0f, 1.0f);
                syncedSource.pitch = Mathf.Clamp(syncedSource.pitch * audioProperties.pitch, 0.0f, 1.0f);
                syncedSource.PlayOneShot(audioClips[clip]);

                if (audibleRange > 0.0f)
                {
                    RoundManager.Instance.PlayAudibleNoise(transform.position, audibleRange, !volumeIsLoudness
                        ? audioProperties.loudness : syncedSource.volume, 0,
                        // item.isInElevator && StartOfRound.Instance.hangarDoorsClosed, 0);
                        false, 0); // TODO: Check ship bounds for inside closed ship.

                    /* if (loudness >= 0.6f && item.playerHeldBy != null)
                    {
                        item.playerHeldBy.timeSinceMakingLoudNoise = 0.0f;
                    } */
                }

                if (transmitOverWalkie)
                {
                    WalkieTalkie.TransmitOneShotAudio(syncedSource, audioClips[clip], overrideWalkieVolume
                        ? audioProperties.walkieVolume : syncedSource.volume);
                }

                syncedSource.volume = originalVolume;
                syncedSource.pitch = originalPitch;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PlayOneShotRandom()
        {
            if (audioClips?.Length > 0)
            {
                PlayOneShot(UnityEngine.Random.Range(0, audioClips.Length));
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PlayOneShotRandomLocal()
        {
            if (audioClips?.Length > 0)
            {
                PlayOneShotLocal(UnityEngine.Random.Range(0, audioClips.Length), RollRandomizedProperties());
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        private SyncedAudioProperties RollRandomizedProperties()
        {
            return new SyncedAudioProperties()
            {
                volume = UnityEngine.Random.Range(minVolume, maxVolume),
                pitch = UnityEngine.Random.Range(minPitch, maxPitch),
                loudness = UnityEngine.Random.Range(minLoudness, maxLoudness),
                walkieVolume = UnityEngine.Random.Range(minWalkieVolume, maxWalkieVolume)
            };
        }
    }
}