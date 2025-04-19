using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Networking
{
    /// <summary>
    ///     Simple networking for AudioSource components.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
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
        [Header("Pitch")]
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
        [Header("Walkie")]
        [Tooltip("")]
        public bool transmitOverWalkie = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float walkieVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost && syncedSource?.playOnAwake == true)
            {
                StartOfRound.Instance?.StartNewRoundEvent.AddListener(PlayNetworkedAudio);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            if (IsHost && syncedSource?.playOnAwake == true)
            {
                StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(PlayNetworkedAudio);
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
        public void PlayNetworkedAudio()
        {
            if (syncedSource?.clip != null)
            {
                float pitch = GetRandomPitch();

                PlayAudioLocal(pitch);
                PlayAudioServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>(), pitch);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="pitch"></param>
        private void PlayAudioLocal(float pitch)
        {
            if (syncedSource?.clip != null)
            {
                syncedSource.pitch = pitch;
                syncedSource.Play();

                if (transmitOverWalkie)
                {
                    WalkieTalkie.TransmitOneShotAudio(syncedSource, syncedSource.clip, walkieVolume);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        /// <param name="pitch"></param>
        [ServerRpc(RequireOwnership = false)]
        public void PlayAudioServerRpc(NetworkObjectReference playerWhoCalled, float pitch)
        {
            PlayAudioClientRpc(playerWhoCalled, pitch);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        /// <param name="pitch"></param>
        [ClientRpc]
        public void PlayAudioClientRpc(NetworkObjectReference playerWhoCalled, float pitch)
        {
            if (playerWhoCalled.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                PlayAudioLocal(pitch);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void PlayNetworkedOneshotRandom()
        {
            if (syncedSource != null && audioClips.Count > 0)
            {
                int clip = Random.Range(0, audioClips.Count);
                float pitch = GetRandomPitch();

                PlayOneshotLocal(clip, pitch);
                PlayOneshotServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>(), clip, pitch);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="clip"></param>
        public void PlayNetworkedOneshot(int clip)
        {
            if (syncedSource != null && audioClips.Count > 0)
            {
                float pitch = GetRandomPitch();

                PlayOneshotLocal(clip, pitch);
                PlayOneshotServerRpc(GameNetworkManager.Instance.localPlayerController.GetComponent<NetworkObject>(), clip, pitch);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="pitch"></param>
        private void PlayOneshotLocal(int clip, float pitch)
        {
            if (syncedSource != null && audioClips.Count > clip && audioClips[clip] != null)
            {
                syncedSource.pitch = pitch;
                syncedSource.PlayOneShot(audioClips[clip]);

                if (transmitOverWalkie)
                {
                    WalkieTalkie.TransmitOneShotAudio(syncedSource, audioClips[clip], walkieVolume);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        /// <param name="clip"></param>
        /// <param name="pitch"></param>
        [ServerRpc(RequireOwnership = false)]
        public void PlayOneshotServerRpc(NetworkObjectReference playerWhoCalled, int clip, float pitch)
        {
            PlayOneshotClientRpc(playerWhoCalled, clip, pitch);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        /// <param name="clip"></param>
        /// <param name="pitch"></param>
        [ClientRpc]
        public void PlayOneshotClientRpc(NetworkObjectReference playerWhoCalled, int clip, float pitch)
        {
            if (playerWhoCalled.TryGet(out NetworkObject playerNetworkObject)
                && playerNetworkObject.TryGetComponent(out PlayerControllerB player)
                && player.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId)
            {
                PlayOneshotLocal(clip, pitch);
            }
        }
    }
}