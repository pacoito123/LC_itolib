using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AnimationVelocity : NetworkBehaviour, ISeededScript<AnimationVelocity>
    {
        /// <summary>
        ///     Cached instance of the current <c>AnimationVelocity</c> as an <c>ISeededScript</c>, to avoid having to cast. 
        /// </summary>
        public ISeededScript<AnimationVelocity> SeededSelf { get; }

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
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private ActivationTime activationTime = ActivationTime.StartOfRound;

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
        ///     TODO.
        /// </summary>
        private bool performedActivation;

        /// <summary>
        ///     Cache already-cast <c>ISeededScript</c> instance.
        /// </summary>
        private AnimationVelocity()
        {
            SeededSelf = this;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost || performedActivation)
            {
                return;
            }

            InitialSpeed = SeededSelf.GetSeededRandom().Next(minStartingSpeed, maxStartingSpeed);

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
            ChangeSpeedServerRpc(GameNetworkManager.Instance.localPlayerController, targetSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="targetSpeed"></param>
        [ServerRpc(RequireOwnership = false)]
        private void ChangeSpeedServerRpc(NetworkBehaviourReference playerReference, float targetSpeed)
        {
            ChangeSpeedClientRpc(playerReference, targetSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="targetSpeed"></param>
        [ClientRpc]
        private void ChangeSpeedClientRpc(NetworkBehaviourReference playerReference, float targetSpeed)
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
        public void SyncSpeed()
        {
            if (!performedActivation)
            {
                UnsubscribeFromEvents();

                performedActivation = true;
            }

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
        private void SyncSpeedServerRpc(NetworkBehaviourReference playerReference, float initialSpeed)
        {
            SyncSpeedClientRpc(playerReference, initialSpeed);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="initialSpeed"></param>
        [ClientRpc]
        private void SyncSpeedClientRpc(NetworkBehaviourReference playerReference, float initialSpeed)
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

            animator.Play(initialState);
            animator.SetFloat(speedParameter, initialSpeed);
        }

        /// <summary>
        ///     Unsubscribe to the event that may have been subscribed to, depending on the set <c>ActivationTime</c>.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
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
        }
    }
}