using itolib.Behaviours.Effects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     Represents a platform players can grab and hold on to.
    /// </summary>
    public class PlatformGrabbable : PlayerAttachable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(10.0f)]
        [Header("Grabbable Platform")]
        [Tooltip("")]
        [Min(0.0f)]
        public float grabSpeed;

        /// <summary>
        ///     An offset to apply to the player's position while grabbing on to the platform.
        /// </summary>
        [Tooltip("An offset to apply to the player's position while grabbing on to the platform.")]
        public Vector3 playerOffset = Vector3.zero;

        /// <summary>
        ///     Key required to be held for the player to hang on to the platform. See 'UnityEngine.InputSystem.Key' for number values. Leaving it at '-1' allows players to
        ///     remain attached without holding anything, until being detached through other means (e.g. 'detachTimer').
        /// </summary>
        [Header("Controls")]
        [Tooltip("Key required to be held for the player to hang on to the platform. See 'UnityEngine.InputSystem.Key' for number values. Leaving it at '-1' allows "
            + "players to remain attached without holding anything, until being detached through other means (e.g. 'detachTimer').")]
        public string actionToHold = string.Empty;

        /// <summary>
        ///     Allow players to carry two-handed items while grabbing on to the platform.
        /// </summary>
        [Tooltip("Allow players to carry two-handed items while grabbing on to the platform.")]
        public bool allowTwoHanded = false;

        /// <summary>
        ///     Detach the player if an enemy collides with the platform.
        /// </summary>
        [Header("Detach")]
        [Tooltip("Detach the player if an enemy collides with the platform.")]
        public bool detachOnEnemyCollision = false;

        /// <summary>
        ///     Detach the player if the platform collides with a wall.
        /// </summary>
        [Tooltip("Detach the player if the platform collides with a wall.")]
        public bool detachOnWallCollision = false;

        /// <summary>
        ///     Detach the player if the player is in a special animation.
        /// </summary>
        [Tooltip("Detach the player if the player is in a special animation.")]
        public bool detachOnSpecialAnimation = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        private InputAction? playerAction;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Transform platformTransform = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead && (allowTwoHanded || !player.twoHanded)
                && (playerAction == null || playerAction.IsPressed());
            detachCondition = player => player.isPlayerDead || (detachOnEnemyCollision && player.inAnimationWithEnemy)
                || (detachOnSpecialAnimation && player.inSpecialInteractAnimation) || (playerAction != null && !playerAction.IsPressed());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Start()
        {
            // Cache platform transform.
            platformTransform = transform;

            if (actionToHold.Length > 0)
            {
                // Get action (key) that must be held, if one is set.
                playerAction = GameNetworkManager.Instance.localPlayerController.playerActions.m_Movement.FindAction(actionToHold);
            }

            base.Start();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Update()
        {
            // Check if a player is attached to the platform.
            if (attachedPlayer != null)
            {
                // Move attached player to the platform's position, with the configured offset applied.
                attachedPlayerTransform.position = grabSpeed == 0 ? platformTransform.position + playerOffset
                    : Vector3.Lerp(attachedPlayerTransform.position, platformTransform.position + playerOffset, Time.deltaTime * grabSpeed);

                // Reset attached player's fall time to avoid instant death upon colliding with another (solid) object.
                attachedPlayer.ResetFallGravity();
            }

            base.Update();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="collider"></param>
        protected override void OnTriggerEnter(Collider collider)
        {
            // Check if an enemy collided with the platform while the local player is attached.
            if (detachOnEnemyCollision && localPlayerAttached && collider.TryGetComponent(out EnemyAI _))
            {
                // Detach player from the platform if an enemy collides with it.
                DetachPlayerLocal();

                if (!attachLocally)
                {
                    // Detach attached player on all clients.
                    DetachPlayerServerRpc();
                }

                return;
            }

            // Check if the platform has collided with a wall while the local player is attached.
            if (detachOnWallCollision && localPlayerAttached && (collider.gameObject.layer == LayerMask.NameToLayer("Room") // TODO: LayerMask field instead.
                || collider.gameObject.layer == LayerMask.NameToLayer("MiscLevelGeometry")))
            {
                // Detach player from the platform if it collides with a wall.
                DetachPlayerLocal();

                if (!attachLocally)
                {
                    // Detach attached player on all clients.
                    DetachPlayerServerRpc();
                }

                return;
            }

            base.OnTriggerEnter(collider);
        }
    }
}