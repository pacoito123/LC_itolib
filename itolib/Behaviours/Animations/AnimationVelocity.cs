using itolib.Enums;
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
        public float InitialSpeed { get; private set; } = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public float TransitionTimer { get; private set; } = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public float PreviousTarget { get; private set; } = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public float CurrentTarget { get; private set; } = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public bool TargetReached { get; private set; } = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Animation Velocity")]
        [Tooltip("")]
        public Animator? animator;

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
                    SyncSpeedServerRpc();
                    break;
                case ActivationTime.ScrapSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.AddListener(SyncSpeedServerRpc);
                    break;
                case ActivationTime.HazardSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.AddListener(SyncSpeedServerRpc);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.AddListener(SyncSpeedServerRpc);
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
        public void FixedUpdate()
        {
            if (TargetReached)
            {
                return;
            }

            TransitionTimer += Time.fixedDeltaTime;

            float currentSpeed = Mathf.Lerp(PreviousTarget, CurrentTarget, TransitionTimer * stoppingSpeed);
            animator?.SetFloat(speedParameter, currentSpeed);

            if (currentSpeed == CurrentTarget)
            {
                PreviousTarget = currentSpeed;

                TransitionTimer = 0.0f;
                TargetReached = true;
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
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.RemoveListener(SyncSpeedServerRpc);
                    break;
                case ActivationTime.HazardSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.RemoveListener(SyncSpeedServerRpc);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(SyncSpeedServerRpc);
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
        [ServerRpc(RequireOwnership = false)]
        public void ResetSpeedServerRpc()
        {
            ChangeSpeedClientRpc(InitialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="targetSpeed"></param>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeSpeedServerRpc(float targetSpeed)
        {
            ChangeSpeedClientRpc(targetSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="targetSpeed"></param>
        [ClientRpc]
        public void ChangeSpeedClientRpc(float targetSpeed)
        {
            CurrentTarget = targetSpeed;

            TransitionTimer = 0.0f;
            TargetReached = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SyncSpeedServerRpc()
        {
            SyncSpeedClientRpc(InitialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ClientRpc]
        public void SyncSpeedClientRpc(float initialSpeed)
        {
            InitialSpeed = initialSpeed;
            PreviousTarget = initialSpeed;
            CurrentTarget = initialSpeed;

            TransitionTimer = 0.0f;
            TargetReached = true;

            animator?.SetFloat(speedParameter, initialSpeed);
            animator?.Play(initialState);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RerollSpeedServerRpc()
        {
            SyncSpeedClientRpc(Random.Range(minStartingSpeed, maxStartingSpeed));
        }
    }
}