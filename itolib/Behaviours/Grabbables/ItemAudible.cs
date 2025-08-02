using GameNetcodeStuff;
using itolib.Extensions;
using itolib.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemAudible : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Audible")]
        [Tooltip("")]
        [SerializeField] private GrabbableObject item = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Audio Clips")]
        [Tooltip("")]
        [SerializeField] private AudioSource? itemSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioSource? itemSourceFar;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(3.0f)]
        [Tooltip("")]
        [SerializeField] private AudioClip[]? audioClips;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AudioClip[]? audioClipsFar;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(3.0f)]
        [Header("Audio Properties")]
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] private float audibleRange = 65.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float minVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float maxVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(-3.0f, 3.0f)]
        [SerializeField] private float minPitch = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(-3.0f, 3.0f)]
        [SerializeField] private float maxPitch = 1.0f;

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
        [Header("Other")]
        [Tooltip("")]
        [SerializeField] private bool volumeIsLoudness = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool transmitOverWalkie = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool triggerFromElsewhere = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool requireHolding = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        private IEventfulItem? eventfulSelf;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (item == null || !TryGetComponent(out item) || item is not IEventfulItem eventfulItem)
            {
                // TODO: Log warning
                enabled = false;

                return;
            }

            eventfulSelf = eventfulItem;

            if (!triggerFromElsewhere)
            {
                eventfulSelf.OnActivate.AddListener(ItemActivate);
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
        private void ItemActivate(bool used, bool buttonDown)
        {
            if (audioClips == null)
            {
                return;
            }

            if ((requireHolding && (!item.IsOwner || item.playerHeldBy == null)) || (!requireHolding && !IsHost))
            {
                return;
            }

            int clip = Random.Range(0, audioClips.Length);

            int clipFar = -1;
            if (audioClipsFar?.Length > 0)
            {
                clipFar = Random.Range(0, audioClips.Length);
            }

            float volume = Random.Range(minVolume, maxVolume),
                pitch = Random.Range(minPitch, maxPitch);

            float loudness = volume;
            if (!volumeIsLoudness)
            {
                loudness = Random.Range(minLoudness, maxLoudness);
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
        private void PlayNetworkedAudioServerRpc(NetworkBehaviourReference playerWhoCalled, int clip, int clipFar, float volume, float pitch, float loudness)
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
        private void PlayNetworkedAudioClientRpc(NetworkBehaviourReference playerWhoCalled, int clip, int clipFar, float volume, float pitch, float loudness)
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
        private void StopAudioServerRpc(NetworkBehaviourReference playerWhoCalled)
        {
            StopAudioClientRpc(playerWhoCalled);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerWhoCalled"></param>
        [ClientRpc]
        private void StopAudioClientRpc(NetworkBehaviourReference playerWhoCalled)
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