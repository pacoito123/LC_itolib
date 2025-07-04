using GameNetcodeStuff;
using itolib.Extensions;
using LethalLevelLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class SunScreen : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public bool FoundSun { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Sun Screen")]
        [Tooltip("")]
        public MeshRenderer? sunTexture;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        [FormerlySerializedAs("onSunHidden")]
        public UnityEvent<bool> onDungeonEntered = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [FormerlySerializedAs("onSunRevealed")]
        public UnityEvent<bool> onDungeonExited = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            if (sunTexture == null)
            {
                Transform? sunTransform = (TimeOfDay.Instance != null && TimeOfDay.Instance.sunAnimator != null)
                    ? TimeOfDay.Instance.sunAnimator.transform.Find("SunTexture") : null;
                FoundSun = sunTransform != null && sunTransform.TryGetComponent(out sunTexture);
            }
            else
            {
                FoundSun = true;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            if (!FoundSun)
            {
                enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow == null)
            {
                return;
            }

            DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onPlayerEnterDungeon.AddListener(HideSun);
            DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onPlayerExitDungeon.AddListener(RevealSun);

            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.playerTeleportedEvent.AddListener(ToggleSunOnTeleport);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnDisable()
        {
            if (DungeonManager.CurrentExtendedDungeonFlow == null)
            {
                return;
            }

            DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onPlayerEnterDungeon.RemoveListener(HideSun);
            DungeonManager.CurrentExtendedDungeonFlow.DungeonEvents.onPlayerExitDungeon.RemoveListener(RevealSun);

            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.playerTeleportedEvent.RemoveListener(ToggleSunOnTeleport);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reveal"></param>
        public void HideSun(PlayerControllerB player, bool reveal = false)
        {
            if (!player.IsLocalClient() || !player.isPlayerControlled)
            {
                return;
            }

            if (FoundSun && sunTexture != null && sunTexture.enabled != reveal)
            {
                sunTexture.enabled = reveal;
            }

            if (!reveal)
            {
                onDungeonEntered.Invoke(FoundSun);
            }
            else
            {
                onDungeonExited.Invoke(FoundSun);
            }

        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="pair"></param>
        public void HideSun((EntranceTeleport, PlayerControllerB) pair)
        {
            HideSun(pair.Item2, reveal: false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="pair"></param>
        public void RevealSun((EntranceTeleport, PlayerControllerB) pair)
        {
            HideSun(pair.Item2, reveal: true);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public void ToggleSunOnTeleport(PlayerControllerB player)
        {
            HideSun(player, reveal: !player.isInsideFactory);
        }
    }
}