using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AnimationVelocity : NetworkBehaviour
    {
        /// <summary>
        ///     Seeded Random instance initialized with the current map seed.
        /// </summary>
        public static System.Random? Random { get; internal set; }

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
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                Random ??= new(StartOfRound.Instance.randomMapSeed + 33);
                InitialSpeed = minStartingSpeed != maxStartingSpeed ? ((float)Random.NextDouble() * (maxStartingSpeed - minStartingSpeed))
                    + minStartingSpeed : minStartingSpeed;
            }

            StartOfRound.Instance.StartNewRoundEvent.AddListener(SyncSpeedServerRpc);
        }

        private void FixedUpdate()
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
            StartOfRound.Instance.StartNewRoundEvent.RemoveListener(SyncSpeedServerRpc);
            Random = null;

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
    }
}