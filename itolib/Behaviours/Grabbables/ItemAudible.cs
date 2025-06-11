using GameNetcodeStuff;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [RequireComponent(typeof(ItemGrabbable))]
    public class ItemAudible : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Audible")]
        [Tooltip("")]
        public ItemGrabbable item = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Audio Clips")]
        [Tooltip("")]
        public AudioSource? itemSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioSource? itemSourceFar;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(3.0f)]
        [Tooltip("")]
        public AudioClip[]? audioClips;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip[]? audioClipsFar;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(3.0f)]
        [Header("Audio Properties")]
        [Tooltip("")]
        [Min(0.0f)]
        public float audibleRange = 65.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        public float minVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        public float maxVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(-3.0f, 3.0f)]
        public float minPitch = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(-3.0f, 3.0f)]
        public float maxPitch = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        public float minLoudness = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        public float maxLoudness = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Other")]
        [Tooltip("")]
        public bool volumeIsLoudness = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool transmitOverWalkie = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool triggerFromElsewhere = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool requireHolding = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            if (item == null && !TryGetComponent(out item))
            {
                // TODO: Log warning
                enabled = false;

                return;
            }

            if (!triggerFromElsewhere)
            {
                item.onActivate.AddListener(ItemActivate);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ManualActivate()
        {
            ItemActivate(false, false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void StopAudio()
        {
            if (!item.IsOwner || item.playerHeldBy == null)
            {
                return;
            }

            StopAudioLocal();
            StopAudioServerRpc(item.playerHeldBy);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="used"></param>
        /// <param name="buttonDown"></param>
        public void ItemActivate(bool used, bool buttonDown)
        {
            if (audioClips == null)
            {
                return;
            }

            if ((requireHolding && (!item.IsOwner || item.playerHeldBy == null)) || (!requireHolding && !IsHost))
            {
                return;
            }

            int clip = audioClips.Length > 1 ? Random.Range(0, audioClips.Length) : 0;

            int clipFar = -1;
            if (audioClipsFar?.Length > 0)
            {
                clipFar = audioClipsFar.Length > 1 ? Random.Range(0, audioClips.Length) : 0;
            }

            float volume = (minVolume < maxVolume) ? Random.Range(minVolume, maxVolume) : minVolume;
            float pitch = (minPitch < maxPitch) ? Random.Range(minPitch, maxPitch) : minPitch;

            float loudness = volume;
            if (!volumeIsLoudness)
            {
                loudness = (minLoudness < maxLoudness) ? Random.Range(minLoudness, maxLoudness) : minLoudness;
            }

            PlayNetworkedAudio(clip, clipFar, volume, pitch, loudness);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="clipFar"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="loudness"></param>
        public void PlayNetworkedAudio(int clip, int clipFar = -1, float volume = 1.0f, float pitch = 1.0f, float loudness = 0.0f)
        {
            PlayAudioLocal(clip, clipFar, volume, pitch, loudness);
            PlayNetworkedAudioServerRpc(GameNetworkManager.Instance.localPlayerController,
                clip, clipFar, volume, pitch, loudness);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        /// <param name="clip"></param>
        /// <param name="clipFar"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="loudness"></param>
        [ServerRpc(RequireOwnership = false)]
        public void PlayNetworkedAudioServerRpc(NetworkBehaviourReference playerWhoCalled, int clip, int clipFar, float volume, float pitch, float loudness)
        {
            PlayNetworkedAudioClientRpc(playerWhoCalled, clip, clipFar, volume, pitch, loudness);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        /// <param name="clip"></param>
        /// <param name="clipFar"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="loudness"></param>
        [ClientRpc]
        public void PlayNetworkedAudioClientRpc(NetworkBehaviourReference playerWhoCalled, int clip, int clipFar, float volume, float pitch, float loudness)
        {
            if (playerWhoCalled.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                PlayAudioLocal(clip, clipFar, volume, pitch, loudness);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="clipFar"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="loudness"></param>
        private void PlayAudioLocal(int clip, int clipFar = -1, float volume = 1.0f, float pitch = 1.0f, float loudness = 0.5f)
        {
            if (itemSource != null && audioClips?[clip] != null)
            {
                itemSource.pitch = pitch;
                itemSource.PlayOneShot(audioClips[clip], volume);

                if (itemSourceFar != null && audioClipsFar?[clipFar] != null)
                {
                    itemSourceFar.pitch = pitch;
                    itemSourceFar.PlayOneShot(audioClipsFar[clipFar], volume);
                }

                if (transmitOverWalkie)
                {
                    WalkieTalkie.TransmitOneShotAudio(itemSource, audioClips[clip], volume);
                }

                if (loudness > 0.0f)
                {
                    RoundManager.Instance.PlayAudibleNoise(item.transform.position, audibleRange, loudness, 0,
                        item.isInElevator && StartOfRound.Instance.hangarDoorsClosed, 0);

                    if (loudness >= 0.6f && item.playerHeldBy != null)
                    {
                        item.playerHeldBy.timeSinceMakingLoudNoise = 0.0f;
                    }
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        [ServerRpc(RequireOwnership = false)]
        public void StopAudioServerRpc(NetworkBehaviourReference playerWhoCalled)
        {
            StopAudioClientRpc(playerWhoCalled);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        [ClientRpc]
        public void StopAudioClientRpc(NetworkBehaviourReference playerWhoCalled)
        {
            if (playerWhoCalled.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                StopAudioLocal();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void StopAudioLocal()
        {
            if (itemSource != null && itemSource.isPlaying)
            {
                itemSource.Stop();
            }

            if (itemSourceFar != null && itemSourceFar.isPlaying)
            {
                itemSourceFar.Stop();
            }
        }
    }
}