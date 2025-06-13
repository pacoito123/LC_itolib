using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AnimationVelocity : NetworkBehaviour
    {
        /// <summary>
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random SeededRandom { get; internal set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public float InitialSpeed { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Animation Velocity")]
        [Tooltip("")]
        public Animator animator = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string initialState = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string speedParameter = "";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float minStartingSpeed = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float maxStartingSpeed = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float stoppingSpeed = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public float transitionTimer;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public bool targetReached = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public float previousTarget;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public float currentTarget;

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

            SeededRandom ??= new(StartOfRound.Instance.randomMapSeed + 33);
            InitialSpeed = minStartingSpeed != maxStartingSpeed ? ((float)SeededRandom.NextDouble() * (maxStartingSpeed - minStartingSpeed))
                + minStartingSpeed : minStartingSpeed;

            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    SyncSpeed();
                    break;
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.AddListener(SyncSpeed);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.AddListener(SyncSpeed);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.AddListener(SyncSpeed);
                    }
                    break;
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
        {
            if (animator == null && !TryGetComponent(out animator))
            {
                Plugin.StaticLogger.LogWarning($"Could not find Animator for AnimationVelocity component in GameObject '{gameObject.name}'.");
                enabled = false;

                return;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void FixedUpdate()
        {
            if (targetReached)
            {
                // Disable update loop.
                enabled = false;

                return;
            }

            transitionTimer += Time.fixedDeltaTime;

            float currentSpeed = Mathf.Lerp(previousTarget, currentTarget, transitionTimer * stoppingSpeed);
            animator.SetFloat(speedParameter, currentSpeed);

            if (currentSpeed == currentTarget)
            {
                previousTarget = currentSpeed;

                transitionTimer = 0.0f;
                targetReached = true;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnDestroy()
        {
            SeededRandom = null!;

            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedScrapObjects.RemoveListener(SyncSpeed);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents.onSpawnedMapObjects.RemoveListener(SyncSpeed);
                    break;
                case ActivationTime.StartOfRound:
                    if (StartOfRound.Instance != null)
                    {
                        StartOfRound.Instance.StartNewRoundEvent.RemoveListener(SyncSpeed);
                    }
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }

            base.OnDestroy();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ResetSpeed()
        {
            ChangeSpeed(InitialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void ResetSpeedLocal()
        {
            ChangeSpeedLocal(InitialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="targetSpeed"></param>
        public void ChangeSpeed(float targetSpeed)
        {
            ChangeSpeedLocal(targetSpeed);
            ChangeSpeedServerRpc(GameNetworkManager.Instance.localPlayerController, targetSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="targetSpeed"></param>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeSpeedServerRpc(NetworkBehaviourReference playerReference, float targetSpeed)
        {
            ChangeSpeedClientRpc(playerReference, targetSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="targetSpeed"></param>
        [ClientRpc]
        public void ChangeSpeedClientRpc(NetworkBehaviourReference playerReference, float targetSpeed)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                ChangeSpeedLocal(targetSpeed);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="targetSpeed"></param>
        public void ChangeSpeedLocal(float targetSpeed)
        {
            currentTarget = targetSpeed;

            transitionTimer = 0.0f;
            targetReached = false;

            // Enable update loop.
            enabled = true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void RerollSpeed()
        {
            SyncSpeed(Random.Range(minStartingSpeed, maxStartingSpeed));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SyncSpeed()
        {
            SyncSpeed(InitialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void SyncSpeed(float initialSpeed)
        {
            SyncSpeedLocal(initialSpeed);
            SyncSpeedServerRpc(GameNetworkManager.Instance.localPlayerController, initialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SyncSpeedServerRpc(NetworkBehaviourReference playerReference, float initialSpeed)
        {
            SyncSpeedClientRpc(playerReference, initialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="initialSpeed"></param>
        [ClientRpc]
        public void SyncSpeedClientRpc(NetworkBehaviourReference playerReference, float initialSpeed)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SyncSpeedLocal(initialSpeed);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="initialSpeed"></param>
        public void SyncSpeedLocal(float initialSpeed)
        {
            InitialSpeed = initialSpeed;
            previousTarget = initialSpeed;
            currentTarget = initialSpeed;

            transitionTimer = 0.0f;
            targetReached = true;

            animator.SetFloat(speedParameter, initialSpeed);
            animator.Play(initialState);
        }
    }
}