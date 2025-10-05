using GameNetcodeStuff;
using itolib.Extensions;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     Represents a platform that can collapse, either due to player carry weight or a flat percentage chance.
    /// </summary>
    public class PlatformUnstable : NetworkBehaviour // TODO: Redo with PlayerAttachable.
    {
        /// <summary>
        ///     Map-seeded (local) Random instance for randomization purposes.
        /// </summary>
        public static System.Random? Random { get; private set; }

        /// <summary>
        ///     Whether or not the platform has collapsed.
        /// </summary>
        public bool PlatformCollapsed { get; private set; }

        /// <summary>
        ///     Whether or not the local player is on top of the platform.
        /// </summary>
        public bool LocalPlayerOnPlatform { get; private set; }

        /// <summary>
        ///     Chance for the platform to collapse every interval.
        /// </summary>
        public int CollapseChance { get; private set; }

        /// <summary>
        ///     Time passed since last collapse interval.
        /// </summary>
        public float TimeSinceLastCheck { get; private set; }

        /// <summary>
        ///     Interval in seconds between each collapse check.
        /// </summary>
        [Tooltip("")]
        public float collapseChanceInterval = 5.0f;

        /// <summary>
        ///     Duration of the collapse animation in seconds, for playing any effects or animations. Can be left at 0 to immediately destroy or start the respawn timer.
        /// </summary>
        [Tooltip("")]
        public float collapseTimer = 0.0f;

        /// <summary>
        ///     Base chance for the platform to collapse (regardless of player weight). Can be left at 0 to 
        /// </summary>
        [Tooltip("")]
        [Range(0, 100)]
        public int baseCollapseChance = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 1.0f)]
        public float weightMultiplier = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool respawnPlatform = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float respawnTimer = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool shakePlayerScreen = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0, 3)]
        public int shakeAmount = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0, 3)]
        public int collapseShakeAmount = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public GameObject? platformContainer;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Animator? platformAnimator;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string shakeState = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string collapseState = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string respawnState = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioSource? platformSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip? shakeSFX;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip? collapseSFX;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip? respawnSFX;

        private void Awake()
        {
            Random = new(StartOfRound.Instance.randomMapSeed + (int)GameNetworkManager.Instance.localPlayerController.actualClientId);
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (PlatformCollapsed)
            {
                return;
            }

            if (collider.TryGetComponent(out VehicleCollisionTrigger _))
            {
                PlatformCollapsed = true;

                if (platformSource != null && collapseSFX != null)
                {
                    platformSource.PlayOneShot(collapseSFX);
                }

                if (platformAnimator != null && collapseState.Length > 0)
                {
                    platformAnimator.Play(collapseState);
                }

                _ = StartCoroutine(CollapsePlatform());

                return;
            }

            if (collider.CompareTag("Player") && collider.TryGetComponent(out PlayerControllerB player) && player.IsLocalClient())
            {
                LocalPlayerOnPlatform = true;

                CollapseChance = baseCollapseChance + (int)((player.carryWeight - 1) * weightMultiplier * 105f);
            }
        }

        private void OnTriggerStay(Collider collider)
        {
            if (PlatformCollapsed || !LocalPlayerOnPlatform)
            {
                return;
            }

            if (TimeSinceLastCheck <= collapseChanceInterval)
            {
                TimeSinceLastCheck += Time.deltaTime;

                return;
            }

            if (Random?.Next(0, 100) < CollapseChance)
            {
                CollapsePlatformLocal();

                if (IsSpawned)
                {
                    ShakePlatformRpc(collapse: true);
                }
            }
            else
            {
                ShakePlatformLocal();

                if (IsSpawned)
                {
                    ShakePlatformRpc();
                }

                TimeSinceLastCheck = 0.0f;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (!PlatformCollapsed && LocalPlayerOnPlatform && collider.CompareTag("Player") && collider.TryGetComponent(out PlayerControllerB player)
                && player.IsLocalClient())
            {
                LocalPlayerOnPlatform = false;
                TimeSinceLastCheck = 0.0f;
                CollapseChance = 0;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ShakePlatformLocal()
        {
            if (shakePlayerScreen && LocalPlayerOnPlatform)
            {
                HUDManager.Instance.ShakeCamera((ScreenShakeType)shakeAmount);
            }

            if (platformSource != null && shakeSFX != null)
            {
                platformSource.PlayOneShot(shakeSFX);
            }

            if (platformAnimator != null && shakeState.Length > 0)
            {
                platformAnimator.Play(shakeState);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void CollapsePlatformLocal()
        {
            PlatformCollapsed = true;

            if (shakePlayerScreen && LocalPlayerOnPlatform)
            {
                HUDManager.Instance.ShakeCamera((ScreenShakeType)collapseShakeAmount);
            }

            if (platformSource != null && collapseSFX != null)
            {
                platformSource.PlayOneShot(collapseSFX);
            }

            if (platformAnimator != null && collapseState.Length > 0)
            {
                platformAnimator.Play(collapseState);
            }

            _ = StartCoroutine(CollapsePlatform());
        }

        private IEnumerator CollapsePlatform()
        {
            yield return new WaitForSeconds(collapseTimer);
            if (platformContainer != null)
            {
                platformContainer.SetActive(false);
            }

            if (!respawnPlatform)
            {
                enabled = false;
                gameObject.SetActive(false);
                yield break;
            }

            yield return new WaitForSeconds(respawnTimer);
            if (platformContainer != null)
            {
                platformContainer.SetActive(true);
            }

            if (platformAnimator != null && respawnState.Length > 0)
            {
                platformAnimator.Play(respawnState);
            }

            if (platformSource != null && respawnSFX != null)
            {
                platformSource.PlayOneShot(respawnSFX);
            }

            LocalPlayerOnPlatform = false;
            TimeSinceLastCheck = 0.0f;
            CollapseChance = 0;

            PlatformCollapsed = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        public void ShakePlatformRpc(bool collapse = false)
        {
            if (!collapse)
            {
                ShakePlatformLocal();
            }
            else if (!PlatformCollapsed)
            {
                CollapsePlatformLocal();
            }
        }
    }
}