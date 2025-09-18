using itolib.Behaviours.Effects;
using itolib.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     Represents a platform players can grab and hold on to.
    /// </summary>
    /// <remarks>Intended for stuff like swinging bars, ziplines, or anything that needs to transport the player somewhere.</remarks>
    public class PlatformGrabbable : PlayerAttachable
    {
        /// <summary>
        ///     Ramping speed for moving the attached player towards the target position.
        /// </summary>
        /// <remarks>If left at <c>0</c>, the player will be immediately moved to the target position without any smoothing.</remarks>
        [Space(10.0f)]
        [Header("Platform Grabbable")]
        [Tooltip("Ramping speed for moving the attached player towards the target position. If left at '0', the player will be immediately moved to the target "
            + "position without any smoothing.")]
        [Min(0.0f)]
        [SerializeField] private float grabSpeed;

        /// <summary>
        ///     Offset to apply to the player's position while grabbing on to the platform.
        /// </summary>
        [Tooltip("Offset to apply to the player's position while grabbing on to the platform.")]
        [SerializeField] private Vector3 playerOffset = Vector3.zero;

        /// <summary>
        ///     <c>PlayerAction</c> required to be held for the player to hang on to the platform. See <c>PlayerActions.MovementActions</c> for a list of valid player actions.
        /// </summary>
        /// <remarks>Leaving it blank allows players to remain attached without holding anything, until being detached through other means (e.g. <c>detachTimer</c>).</remarks>
        [Header("Controls")]
        [Tooltip("PlayerAction required to be held for the player to hang on to the platform. See 'PlayerActions.MovementActions' for a list of valid player "
            + "actions. Leaving it blank allows players to remain attached without holding anything, until being detached through other means (e.g. detach timer).")]
        [SerializeField] private string actionToHold = string.Empty;

        /// <summary>
        ///     Whether to allow players to carry two-handed items while grabbing on to the platform or not.
        /// </summary>
        [Tooltip("Whether to allow players to carry two-handed items while grabbing on to the platform or not.")]
        [SerializeField] private bool allowTwoHanded;

        /// <summary>
        ///     Whether to detach the player if an enemy collides with the platform or not.
        /// </summary>
        [Header("Detach")]
        [Tooltip("Whether to detach the player if an enemy collides with the platform or not.")]
        [SerializeField] private bool detachOnEnemyCollision;

        /// <summary>
        ///     Whether to detach the player if the platform collides with a wall or not.
        /// </summary>
        [Tooltip("Whether to detach the player if the platform collides with a wall or not.")]
        [SerializeField] private bool detachOnWallCollision;

        /// <summary>
        ///     Whether to detach the player if the player enters a special animation or not.
        /// </summary>
        [Tooltip("Whether to detach the player if the player enters a special animation or not.")]
        [SerializeField] private bool detachOnSpecialAnimation = true;

        /// <summary>
        ///     Cached reference to the player action to be held in order to remain attached to the platform.
        /// </summary>
        private InputAction? playerAction;

        /// <summary>
        ///     Cached transform for this platform.
        /// </summary>
        private Transform platformTransform = null!;

        /// <summary>
        ///     Attach if the player is alive, not holding a two-handed item (<c>allowTwoHanded</c>), and the player action is being held (<c>playerAction</c>).
        ///     Detach if the player is dead, collides with an enemy (<c>detachOnEnemyCollision</c>), enters a special animation (<c>detachOnSpecialAnimation</c>),
        ///         or stops holding the player action (<c>playerAction</c>).
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead && (allowTwoHanded || !player.twoHanded)
                && (actionToHold.Length == 0 || (playerAction != null && playerAction.IsPressed()));
            detachCondition = player => player.isPlayerDead || (detachOnEnemyCollision && player.inAnimationWithEnemy) || (detachOnSpecialAnimation
                && player.inSpecialInteractAnimation) || (actionToHold.Length > 0 && (playerAction == null || !playerAction.IsPressed()));
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected override void Start()
        {
            // Cache platform transform.
            platformTransform = transform;

            // Try obtain player action required to be held.
            if (actionToHold.Length > 0 && !GameNetworkManager.Instance.localPlayerController.TryFindMovementAction(out playerAction, actionToHold))
            {
                Plugin.StaticLogger.LogWarning($"Could not find movement action '{actionToHold}' defined for PlatformGrabbable component in '{transform.name}'!");
            }

            base.Start();
        }

        /// <summary>
        ///     Handle transporting player who is attached to the platform.
        /// </summary>
        protected override void Update()
        {
            // Check if a player is attached to the platform.
            if (attachedPlayer != null)
            {
                // Move attached player to the platform's position, with the configured offset applied.
                attachedPlayerTransform.position = grabSpeed == 0.0f ? platformTransform.position + playerOffset
                    : Vector3.Lerp(attachedPlayerTransform.position, platformTransform.position + playerOffset, Time.deltaTime * grabSpeed);

                // Reset attached player's fall time to avoid instant death upon colliding with another (solid) object.
                attachedPlayer.ResetFallGravity();
            }

            base.Update();
        }

        /// <summary>
        ///     Handle platform collisions checks to detach player, for <c>detachOnEnemyCollision</c> and <c>detachOnWallCollision</c>.
        /// </summary>
        /// <param name="collider"><c>Collider</c> to check.</param>
        protected override void OnTriggerEnter(Collider collider)
        {
            // Check if an enemy collided with the platform while the local player is attached.
            if (detachOnEnemyCollision && localPlayerAttached && collider.TryGetComponent(out EnemyAI _))
            {
                // Detach player from the platform if an enemy collides with it.
                DetachPlayer();

                return;
            }

            // Check if the platform has collided with a wall while the local player is attached.
            if (detachOnWallCollision && localPlayerAttached && (collider.gameObject.layer == LayerMask.NameToLayer("Room") // TODO: LayerMask field instead.
                || collider.gameObject.layer == LayerMask.NameToLayer("MiscLevelGeometry")))
            {
                // Detach player from the platform if the platform collides with a wall.
                DetachPlayer();

                return;
            }

            base.OnTriggerEnter(collider);
        }
    }
}