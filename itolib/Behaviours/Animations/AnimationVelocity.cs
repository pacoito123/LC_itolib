using DunGen;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace itolib.Behaviours.Animations
{
    /// <summary>
    ///     Smoothly changes an <c>Animator</c>'s defined <i>speed</i> multiplier parameter to a desired value, and syncs it across clients.
    /// </summary>
    public sealed class AnimationVelocity : NetworkBehaviour, IActivationScript, ISeededScript<AnimationVelocity>
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
        ///     <c>Animator</c> with a <i>speed</i> multiplier parameter to target.
        /// </summary>
        [Header("Animation Velocity")]
        [Tooltip("Animator with a speed multiplier parameter to target.")]
        [SerializeField] private Animator animator;

        /// <summary>
        ///     Approximate time until reaching the target speed, in seconds.
        /// </summary>
        [Tooltip("Approximate time until reaching the target speed, in seconds.")]
        [Min(0.0f)]
        [FormerlySerializedAs("stoppingSpeed")]
        [SerializeField] private float transitionTime = 1.0f;

        /// <summary>
        ///     Name of the <c>Animator</c> state to begin playing after syncing speed with clients.
        /// </summary>
        [Tooltip("Name of the Animator state to begin playing after syncing speed with clients.")]
        [SerializeField] private string initialState = string.Empty;

        /// <summary>
        ///     Name of the <i>speed</i> multiplier parameter defined in the <c>Animator</c>.
        /// </summary>
        /// <remarks><b>NOTE:</b> Needs to be set as the speed multiplier value of the <c>Animator</c> state.</remarks>
        [Tooltip("Name of the speed multiplier parameter defined in the Animator. NOTE: Needs to be set as the speed multiplier value of the Animator state.")]
        [SerializeField] private string speedParameter = string.Empty;

        /// <summary>
        ///     Minimum value for the <i>speed</i> multiplier parameter when rolling a random value.
        /// </summary>
        [Tooltip("Minimum value for the speed multiplier parameter when rolling a random value.")]
        [SerializeField] private float minStartingSpeed = 1.0f;

        /// <summary>
        ///     Maximum value for the <i>speed</i> multiplier parameter when rolling a random value.
        /// </summary>
        [Tooltip("Maximum value for the speed multiplier parameter when rolling a random value.")]
        [SerializeField] private float maxStartingSpeed = 1.0f;

        /// <summary>
        ///     Whether the random <i>speed</i> multiplier parameter rolling should be seeded or not.
        /// </summary>
        [Tooltip("Whether the random speed multiplier parameter rolling should be seeded or not.")]
        [SerializeField] private bool isSeededRandom = true;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the initial speed sync.
        /// </summary>
        [field: Tooltip("Desired activation time for the initial speed sync.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.StartOfRound;

        /// <summary>
        ///     Hash of the <c>Animator</c> state to begin playing after syncing speed with clients.
        /// </summary>
        private int initialStateID;

        /// <summary>
        ///     Hash of the <i>speed</i> multiplier parameter defined in the <c>Animator</c>.
        /// </summary>
        private int speedParameterID;

        /// <summary>
        ///     Current or initial velocity for the <i>speed</i> multiplier parameter.
        /// </summary>
        private float transitionVelocity;

        /// <summary>
        ///     Whether the target <i>speed</i> multiplier parameter value has been reached or not.
        /// </summary>
        private bool targetReached = true;

        /// <summary>
        ///     Whether the current target <i>speed</i> is allowed to be changed or not.
        /// </summary>
        private bool targetLocked;

        /// <summary>
        ///     Starting or initial target value of the <i>speed</i> multiplier parameter, to reset back to.
        /// </summary>
        private float startingSpeed;

        /// <summary>
        ///     Current value of the <i>speed</i> multiplier parameter.
        /// </summary>
        private float currentSpeed;

        /// <summary>
        ///     Target value for the <i>speed</i> multiplier parameter.
        /// </summary>
        private float targetSpeed;

        /// <summary>
        ///     Speed of the transition between current speed and target speed.
        /// </summary>
        /// <remarks>Deprecated. Should be ignored.</remarks>
        [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Deprecated.")]
        private float stoppingSpeed = -1.0f;

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> and <c>ISeededScript</c> instances.
        /// </summary>
        private AnimationVelocity()
        {
            ActivationSelf = this;
            SeededSelf = this;

            animator = null!;
        }

        /// <summary>
        ///     Hash defined <i>speed</i> parameter and <c>Animator</c> state names.
        /// </summary>
        private void Awake()
        {
            initialStateID = Animator.StringToHash(initialState);
            speedParameterID = Animator.StringToHash(speedParameter);

            // Convert deprecated stopping speed to transition time.
            if (stoppingSpeed > 0.0f)
            {
                transitionTime = 1 / stoppingSpeed;
            }
        }

        /// <summary>
        ///     Roll initial value for the <c>Animator</c>'s <i>speed</i> parameter.
        /// </summary>
        private void Start()
        {
            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     Make sure there is an <c>Animator</c> component to target.
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
        ///     Handle smooth transition between <i>speed</i> multiplier parameter values.
        /// </summary>
        private void FixedUpdate()
        {
            if (targetReached)
            {
                // Disable update loop if the target speed is reached.
                enabled = false;

                return;
            }

            // Move towards the target speed and set current parameter value.
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref transitionVelocity, transitionTime);
            animator.SetFloat(speedParameterID, currentSpeed);

            if (currentSpeed == targetSpeed)
            {
                // Set target speed as reached.
                targetReached = true;
            }
        }

        /// <summary>
        ///     Unsubscribe from any events that may have been subscribed to.
        /// </summary>
        public override void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();

            base.OnDestroy();
        }

        /// <summary>
        ///     Reset <i>speed</i> multiplier parameter to its starting or initial target value.
        /// </summary>
        public void ResetSpeed()
        {
            ChangeSpeed(startingSpeed);
        }

        /// <summary>
        ///     Roll and set a new <i>speed</i> multiplier parameter value, within the specified range.
        /// </summary>
        public void RerollSpeed()
        {
            RerollSpeed(restart: false);
        }

        /// <summary>
        ///     Roll and set a new <i>speed</i> multiplier parameter value, within the specified range.
        /// </summary>
        /// <param name="restart">Whether to use the rolled number as the starting or initial target value or not, which also resets the <c>Animator</c> state.</param>
        public void RerollSpeed(bool restart)
        {
            float targetSpeed = (minStartingSpeed >= maxStartingSpeed) ? minStartingSpeed : (isSeededRandom
                ? SeededSelf.GetSeededRandom().Next(minStartingSpeed, maxStartingSpeed)
                : Random.Range(minStartingSpeed, maxStartingSpeed));

            if (!restart)
            {
                // Change target speed to the rolled value.
                ChangeSpeed(targetSpeed);
            }
            else
            {
                // Restart Animator with the rolled value as the starting or initial target speed.
                SyncSpeed(targetSpeed);
            }
        }

        /// <summary>
        ///     Change target value for the <i>speed</i> multiplier parameter.
        /// </summary>
        /// <param name="targetSpeed">Target speed value to change to.</param>
        public void ChangeSpeed(float targetSpeed)
        {
            if (!PerformedActivation)
            {
                // Sync speed with clients, if not already done.
                SyncSpeed(targetSpeed);

                return;
            }

            // Check if target is locked, or already at the target speed.
            if (targetLocked || this.targetSpeed == targetSpeed)
            {
                return;
            }

            // Check if object is spawned.
            if (IsSpawned)
            {
                // Change target speed for all clients to the given value.
                ChangeSpeedRpc(targetSpeed);
            }
            else
            {
                // Change target speed for the local client to the given value.
                ChangeSpeedLocal(targetSpeed);
            }
        }

        /// <summary>
        ///     Change target value for the <i>speed</i> multiplier parameter for all clients.
        /// </summary>
        /// <param name="targetSpeed">Target speed value to change to.</param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void ChangeSpeedRpc(float targetSpeed)
        {
            // Change target speed for the local client.
            ChangeSpeedLocal(targetSpeed);
        }

        /// <summary>
        ///     Change target value for the <i>speed</i> multiplier parameter for the local client.
        /// </summary>
        /// <param name="targetSpeed">Target speed value to change to.</param>
        public void ChangeSpeedLocal(float targetSpeed)
        {
            // Check if target is locked, or already at the target speed.
            if (targetLocked || this.targetSpeed == targetSpeed)
            {
                return;
            }

            // Set new target speed.
            this.targetSpeed = targetSpeed;
            targetReached = false;

            // Enable update loop.
            enabled = true;
        }

        /// <summary>
        ///     Set starting or initial target for the <i>speed</i> multiplier parameter.
        /// </summary>
        /// <param name="startingSpeed">Starting or initial target speed value to use.</param>
        public void SyncSpeed(float startingSpeed)
        {
            // Check if already set as the starting or initial speed.
            if (this.startingSpeed == startingSpeed)
            {
                return;
            }

            // Check if object is spawned.
            if (IsSpawned)
            {
                // Send given starting or initial target speed value to the server.
                SyncSpeedServerRpc(startingSpeed);
            }
            else
            {
                // Set starting or initial target speed to the given value, for the local client.
                SyncSpeedLocal(startingSpeed);
            }
        }

        /// <summary>
        ///     Set starting or initial target for the <i>speed</i> multiplier parameter on the server.
        /// </summary>
        /// <param name="startingSpeed">Starting or initial target speed value to use.</param>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SyncSpeedServerRpc(float startingSpeed)
        {
            // Set starting or initial target speed to the given value, for all clients.
            SyncSpeedClientRpc(startingSpeed);
        }

        /// <summary>
        ///     Set starting or initial target for the <i>speed</i> multiplier parameter on all clients.
        /// </summary>
        /// <param name="startingSpeed">Starting or initial target speed value to use.</param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)] // TODO: Consider DeferLocal but only with other clients connected.
        private void SyncSpeedClientRpc(float startingSpeed)
        {
            // Set starting or initial target speed to the given value, for the local client.
            SyncSpeedLocal(startingSpeed);
        }

        /// <summary>
        ///     Set starting or initial target for the <i>speed</i> multiplier parameter on the local client.
        /// </summary>
        /// <param name="startingSpeed">Starting or initial target speed value to use.</param>
        public void SyncSpeedLocal(float startingSpeed)
        {
            // Set starting or initial target speed.
            this.startingSpeed = startingSpeed;

            currentSpeed = animator.GetFloat(speedParameterID);
            targetSpeed = startingSpeed;
            targetReached = false;
            targetLocked = false;

            enabled = true;
            // ...

            // Completely reset Animator state when syncing speed with clients:
            animator.Rebind();
            animator.Update(0.0f);
            animator.Play(initialStateID);

            animator.enabled = true;
            // ...

            // Set activation as performed, if not already done.
            PerformedActivation = true;
        }

        /// <summary>
        ///     Change target value for the <i>speed</i> multiplier parameter and lock it.
        /// </summary>
        /// <param name="targetSpeed">Target speed value to change to.</param>
        public void LockSpeed(float targetSpeed)
        {
            if (IsSpawned && !targetLocked)
            {
                LockSpeedRpc(targetSpeed);
            }
        }

        /// <summary>
        ///     Change target value for the <i>speed</i> multiplier parameter and lock it for all clients.
        /// </summary>
        /// <remarks><b>NOTE:</b> Only unlocks when resyncing the entire <c>Animator</c>.</remarks>
        /// <param name="targetSpeed">Target speed value to change to.</param>
        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void LockSpeedRpc(float targetSpeed)
        {
            if (!targetLocked)
            {
                ChangeSpeedLocal(targetSpeed);
                targetLocked = true;
            }
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public void PerformActivation(ActivationTime activationTime)
        {
            float startingSpeed = (minStartingSpeed >= maxStartingSpeed) ? minStartingSpeed : (isSeededRandom
                ? SeededSelf.GetSeededRandom().Next(minStartingSpeed, maxStartingSpeed)
                : Random.Range(minStartingSpeed, maxStartingSpeed));
            SyncSpeed(startingSpeed);
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