using GameNetcodeStuff;
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
        public UnityEvent<bool> onSunHidden = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<bool> onSunRevealed = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            FoundSun = sunTexture != null || (TimeOfDay.Instance?.sunAnimator.transform.Find("SunTexture")?.TryGetComponent(out sunTexture) ?? false);
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

            StartOfRound.Instance?.playerTeleportedEvent.AddListener(ToggleSunOnTeleport);
            StartOfRound.Instance?.CameraSwitchEvent.AddListener(ToggleSunOnSpectatorSwitch);
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

            StartOfRound.Instance?.playerTeleportedEvent.RemoveListener(ToggleSunOnTeleport);
            StartOfRound.Instance?.CameraSwitchEvent.RemoveListener(ToggleSunOnSpectatorSwitch);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="pair"></param>
        public void HideSun((EntranceTeleport, PlayerControllerB) pair)
        {
            ulong playerID = pair.Item2.actualClientId;
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            if (playerID != localPlayer.actualClientId
                || (localPlayer.isPlayerDead && localPlayer.spectatedPlayerScript?.actualClientId != playerID))
            {
                return;
            }

            if (FoundSun && sunTexture?.enabled == true)
            {
                sunTexture.enabled = false;
            }

            onSunHidden.Invoke(FoundSun);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="pair"></param>
        public void RevealSun((EntranceTeleport, PlayerControllerB) pair)
        {
            ulong playerID = pair.Item2.actualClientId;
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            if (playerID != localPlayer.actualClientId
                || (localPlayer.isPlayerDead && localPlayer.spectatedPlayerScript?.actualClientId != playerID))
            {
                return;
            }

            if (FoundSun && sunTexture != null && !sunTexture.enabled)
            {
                sunTexture.enabled = true;
            }

            onSunRevealed.Invoke(FoundSun);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        public void ToggleSunOnTeleport(PlayerControllerB player)
        {
            if (player.isPlayerDead || !player.isPlayerControlled)
            {
                return;
            }

            if (player.isInsideFactory)
            {
                HideSun(new(null!, player));
            }
            else
            {
                RevealSun(new(null!, player));
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ToggleSunOnSpectatorSwitch()
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (!localPlayer.isPlayerDead || StartOfRound.Instance.overrideSpectateCamera
                || localPlayer.spectatedPlayerScript == null || localPlayer.spectatedPlayerScript.isPlayerDead)
            {
                return;
            }

            PlayerControllerB spectatedPlayer = GameNetworkManager.Instance.localPlayerController;
            if (spectatedPlayer.isInsideFactory)
            {
                HideSun(new(null!, spectatedPlayer));
            }
            else
            {
                RevealSun(new(null!, spectatedPlayer));
            }
        }
    }
}