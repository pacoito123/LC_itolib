using GameNetcodeStuff;
using itolib.Extensions;
using LethalLevelLoader;
using UnityEngine;
using UnityEngine.Events;

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
        [Header("Sun Screen")]
        [Tooltip("")]
        [SerializeField] private MeshRenderer? sunTexture;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<bool> onDungeonEntered = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<bool> onDungeonExited = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool foundSun;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (sunTexture == null)
            {
                Transform? sunTransform = (TimeOfDay.Instance != null && TimeOfDay.Instance.sunAnimator != null)
                    ? TimeOfDay.Instance.sunAnimator.transform.Find("SunTexture") : null;

                foundSun = sunTransform != null && sunTransform.TryGetComponent(out sunTexture);
            }
            else
            {
                foundSun = true;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Start()
        {
            if (!foundSun)
            {
                enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
        {
            DungeonManager.GlobalDungeonEvents.onPlayerEnterDungeon.AddListener(HideSun);
            DungeonManager.GlobalDungeonEvents.onPlayerExitDungeon.AddListener(RevealSun);

            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.playerTeleportedEvent.AddListener(ToggleSunOnTeleport);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnDisable()
        {
            DungeonManager.GlobalDungeonEvents.onPlayerEnterDungeon.RemoveListener(HideSun);
            DungeonManager.GlobalDungeonEvents.onPlayerExitDungeon.RemoveListener(RevealSun);

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
        private void HideSun(PlayerControllerB player, bool reveal = false)
        {
            if (!player.IsLocalClient() || !player.isPlayerControlled)
            {
                return;
            }

            if (foundSun && sunTexture != null && sunTexture.enabled != reveal)
            {
                sunTexture.enabled = reveal;
            }

            if (!reveal)
            {
                onDungeonEntered.Invoke(foundSun);
            }
            else
            {
                onDungeonExited.Invoke(foundSun);
            }

        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="pair"></param>
        private void HideSun((EntranceTeleport, PlayerControllerB) pair)
        {
            HideSun(pair.Item2, reveal: false);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="pair"></param>
        private void RevealSun((EntranceTeleport, PlayerControllerB) pair)
        {
            HideSun(pair.Item2, reveal: true);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        private void ToggleSunOnTeleport(PlayerControllerB player)
        {
            HideSun(player, reveal: !player.isInsideFactory);
        }
    }
}