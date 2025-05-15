using itolib.Compatibility;
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
        public static TVScript? VanillaTV
        {
            get
            {
                if (field == null && StartOfRound.Instance?.unlockablesList.unlockables.Find(unlockable =>
                    string.CompareOrdinal(unlockable.unlockableName, "Television") == 0)?.prefabObject.transform.Find("TVScript")?
                        .TryGetComponent(out TVScript tvScript) == true)
                {
                    field = tvScript;
                }

                return field;
            }
            private set;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            if (VanillaTV?.TryGetComponent(out VideoPlayer vanillaVideo) == true
                && TryGetComponent(out VideoPlayer video))
            {
                tvClips = VanillaTV.tvClips;
                tvAudioClips = VanillaTV.tvAudioClips;

                tvOnMaterial = VanillaTV.tvOnMaterial;
                tvOffMaterial = VanillaTV.tvOffMaterial;

                switchTVOn = VanillaTV.switchTVOn;
                switchTVOff = VanillaTV.switchTVOff;

                tvMesh.sharedMaterials = VanillaTV.tvMesh.sharedMaterials;

                video.clip = tvClips[0];
                video.targetTexture = vanillaVideo.targetTexture;
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