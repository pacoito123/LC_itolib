using DunGen;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AnimationVelocity : NetworkBehaviour, IActivationScript, ISeededScript<AnimationVelocity>
    {
        /// <summary>
        ///     Cached instance of the current <c>AnimationVelocity</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Cached instance of the current <c>AnimationVelocity</c> as an <c>ISeededScript</c>, to avoid having to cast.
        /// </summary>
        public ISeededScript<AnimationVelocity> SeededSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public float InitialSpeed { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Animation Velocity")]
        [Tooltip("")]
        [SerializeField] private Animator animator = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private string initialState = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private string speedParameter = string.Empty;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private float minStartingSpeed = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private float maxStartingSpeed = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private float stoppingSpeed = 1.0f;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the initial velocity sync.
        /// </summary>
        [field: Tooltip("Desired activation time for the initial velocity sync.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        private float transitionTimer;

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool targetReached = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        private float previousTarget;

        /// <summary>
        ///     TODO.
        /// </summary>
        private float currentTarget;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> and <c>ISeededScript</c> instances.
        /// </summary>
        private AnimationVelocity()
        {
            ActivationSelf = this;
            SeededSelf = this;
        }

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

            InitialSpeed = SeededSelf.GetSeededRandom().Next(minStartingSpeed, maxStartingSpeed);

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void OnEnable()
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
        private void FixedUpdate()
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
            ActivationSelf.UnsubscribeFromEvents();

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
        public void RerollSpeed()
        {
            SyncSpeed(SeededSelf.GetSeededRandom().Next(minStartingSpeed, maxStartingSpeed));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="targetSpeed"></param>
        public void ChangeSpeed(float targetSpeed)
        {
            ChangeSpeedLocal(targetSpeed);

            if (IsSpawned)
            {
                ChangeSpeedRpc(targetSpeed);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="targetSpeed"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void ChangeSpeedRpc(float targetSpeed)
        {
            ChangeSpeedLocal(targetSpeed);
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
        public void SyncSpeed()
        {
            SyncSpeed(InitialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="initialSpeed"></param>
        public void SyncSpeed(float initialSpeed)
        {
            SyncSpeedLocal(initialSpeed);

            if (IsSpawned)
            {
                SyncSpeedRpc(initialSpeed);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="initialSpeed"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void SyncSpeedRpc(float initialSpeed)
        {
            SyncSpeedLocal(initialSpeed);
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

            animator.Play(initialState);
            animator.SetFloat(speedParameter, initialSpeed);
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            SyncSpeed();
        }

        /// <summary>
        ///     <c>DunGen</c> listener called when the Dungeon finishes generating.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            ActivationSelf.OnDungeonComplete();
        }
    }
}