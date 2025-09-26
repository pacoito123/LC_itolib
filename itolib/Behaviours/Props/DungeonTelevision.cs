using itolib.Compatibility;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace itolib.Behaviours.Props
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class DungeonTelevision : TVScript
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public static UnlockableItem? TelevisionUnlockableItem
        {
            get
            {
                if (field == null && StartOfRound.Instance != null)
                {
                    field = StartOfRound.Instance.unlockablesList.unlockables.Find(unlockable =>
                        unlockable.unlockableName.CompareOrdinal("Television"));
                }

                return field;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Dungeon Television")]
        [Tooltip("")]
        [SerializeField] private InteractTrigger? tvTrigger;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool startDeactivated;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (TelevisionUnlockableItem == null)
            {
                return;
            }

            Transform? tvScript = TelevisionUnlockableItem.prefabObject.transform.Find("TVScript");
            if (tvScript.TryGetComponent(out TVScript television)
                && television.TryGetComponent(out VideoPlayer vanillaVideo))
            {
                tvClips = television.tvClips;
                tvAudioClips = television.tvAudioClips;

                tvOnMaterial = television.tvOnMaterial;
                tvOffMaterial = television.tvOffMaterial;

                switchTVOn = television.switchTVOn;
                switchTVOff = television.switchTVOff;

                tvMesh.sharedMaterials = television.tvMesh.sharedMaterials;

                if (TryGetComponent(out VideoPlayer video))
                {
                    video.clip = tvClips[0];
                    video.targetTexture = vanillaVideo.targetTexture;
                }
            }

            if (startDeactivated || (TVLoaderCompatibility.Enabled && (TelevisionUnlockableItem?.alreadyUnlocked == true
                || TelevisionUnlockableItem?.hasBeenUnlockedByPlayer == true)))
            {
                if (tvTrigger != null)
                {
                    tvTrigger.interactable = false;
                }

                enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void OnEnable()
        {
            video.loopPointReached += TVFinishedClip;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void OnDisable()
        {
            video.loopPointReached -= TVFinishedClip;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="source"></param>
        public new void TVFinishedClip(VideoPlayer source)
        {
            if (!tvOn || !GameNetworkManager.Instance.localPlayerController.isInsideFactory)
            {
                return;
            }

            currentClip = (currentClip + 1) % tvClips.Length;
            video.clip = tvClips[currentClip];
            video.Play();

            tvSFX.clip = tvAudioClips[currentClip];
            tvSFX.time = 0f;
            tvSFX.Play();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public new void Update()
        {
            if (NetworkManager.Singleton.ShutdownInProgress || GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }

            if (TVLoaderCompatibility.Enabled)
            {
                TVLoaderCompatibility.PrepareVideo(this);
                return;
            }

            if (!tvOn || !GameNetworkManager.Instance.localPlayerController.isInsideFactory)
            {
                if (wasTvOnLastFrame)
                {
                    wasTvOnLastFrame = false;
                    SetTVScreenMaterial(false);
                    currentClipTime = (float)video.time;
                    video.Stop();
                }

                if (IsHost && !tvOn)
                {
                    timeSinceTurningOffTV += Time.deltaTime;
                }
                currentClipTime += Time.deltaTime;

                if (currentClipTime > tvClips[currentClip].length)
                {
                    currentClip = (currentClip + 1) % tvClips.Length;
                    currentClipTime = 0f;

                    if (tvOn)
                    {
                        tvSFX.clip = tvAudioClips[currentClip];
                        tvSFX.Play();

                        return;
                    }
                }
            }
            else
            {
                if (!wasTvOnLastFrame)
                {
                    wasTvOnLastFrame = true;
                    SetTVScreenMaterial(true);

                    video.clip = tvClips[currentClip];
                    video.time = currentClipTime;
                    video.Play();
                }

                currentClipTime = (float)video.time;
            }
        }
    }
}