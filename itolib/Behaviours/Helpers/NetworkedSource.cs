using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     Simple networking for AudioSource components.
    /// </summary>
    public class NetworkedSource : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Networked Source")]
        [Tooltip("")]
        public AudioSource? syncedSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<AudioClip> audioClips = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float minPitch = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float maxPitch = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost)
            {
                return;
            }

            if (syncedSource?.playOnAwake == true)
            {
                StartOfRound.Instance?.StartNewRoundEvent.AddListener(PlayAudioServerRpc);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            if (syncedSource?.playOnAwake == true)
            {
                StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(PlayAudioServerRpc);
            }

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public float GetRandomPitch()
        {
            return (minPitch != 1.0f && minPitch < maxPitch) ? Random.Range(minPitch, maxPitch)
                : syncedSource?.pitch ?? 1.0f;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void PlayAudioServerRpc()
        {
            PlayAudioClientRpc(GetRandomPitch());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void PlayAudioClientRpc(float pitch)
        {
            if (syncedSource != null)
            {
                syncedSource.pitch = pitch;
                syncedSource.Play();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void PlayOneshotServerRpc(int clip)
        {
            if (audioClips.Count > clip)
            {
                PlayOneshotClientRpc(clip, GetRandomPitch());
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void PlayOneshotClientRpc(int clip, float pitch)
        {
            if (syncedSource != null)
            {
                syncedSource.pitch = pitch;
                syncedSource?.PlayOneShot(audioClips[clip]);
            }
        }
    }
}