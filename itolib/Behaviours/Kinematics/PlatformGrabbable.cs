using GameNetcodeStuff;
using itolib.Behaviours.Effects;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     Represents a platform players can grab and hold on to.
    /// </summary>
    public class PlatformGrabbable : PlayerAttachable // TODO: Deprecate and rewrite some stuff
    {
        /// <summary>
        ///     Synchronized value for the animation variant to play when the player attaches.
        /// </summary>
        public NetworkVariable<int> SyncedStateVariant { get; private set; } = new(-1); // TODO: Can do without a NetworkVariable.

        /// <summary>
        ///     Synchronized pitch value for the platform sound effects.
        /// </summary>
        public NetworkVariable<float> SyncedPitch { get; private set; } = new(1.0f);

        /// <summary>
        ///     Animator instance of the platform, used to play an animation once a player grabs the platform. Optional if already playing an animation,
        ///     or the platform is stationary.
        /// </summary>
        [Space(10f)]
        [Header("Grabbable Platform")]
        [Tooltip("Animator instance of the platform, used to play an animation once a player grabs the platform. Optional if already playing an animation, "
            + "or the platform is stationary.")]
        public Animator? platformAnimator;

        /// <summary>
        ///     Name of the animation state to play once a player grabs the platform. Make sure to leave out any numbers at the end if using animation variants.
        ///     Optional if already playing an animation, or the platform is stationary.
        /// </summary>
        [Tooltip("Name of the animation state to play once a player grabs the platform. Make sure to leave out any numbers at the end if using animation variants. "
            + "Optional if already playing an animation, or the platform is stationary.")]
        public string stateName = string.Empty;

        /// <summary>
        ///     Value for the animation variant to play when the player attaches, which is appended to 'stateName'. Should only be updated in the editor, or in-game
        ///     before spawning the platform. Leaving it at '-1' disables animation variants.
        /// </summary>
        [Tooltip("Value for the animation variant to play when the player attaches, which is appended to 'stateName'. Should only be updated in the editor, "
            + "or in-game before spawning the platform. Leaving it at '-1' disables animation variants.")]
        public int stateVariant = -1;

        /// <summary>
        ///     An offset to apply to the player's position while grabbing on to the platform.
        /// </summary>
        [Tooltip("An offset to apply to the player's position while grabbing on to the platform.")]
        public Vector3 playerOffset = Vector3.zero;

        /// <summary>
        ///     Key required to be held for the player to hang on to the platform. See 'UnityEngine.InputSystem.Key' for number values. Leaving it at '-1' allows players to
        ///     remain attached without holding anything, until being detached through other means (e.g. 'detachTimer').
        /// </summary>
        /// <remarks>Probably worth looking into adding controller support for this.</remarks>
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
        ///     AudioSource instance of the platform, used to play sound effects at various points. Optional if not playing any sound effects.
        /// </summary>
        [Header("Audio")]
        [Tooltip("AudioSource instance of the platform, used to play sound effects at various points. Optional.")]
        public AudioSource? platformSource;

        /// <summary>
        ///     Sound effect to play when the platform spawns. Optional if not playing a spawning sound effect.
        /// </summary>
        /// <remarks>Could be swapped for a list of sounds to pick from randomly.</remarks>
        [Tooltip("Sound effect to play when the platform spawns. Optional if not playing a spawning sound effect.")]
        public AudioClip? spawnSFX;

        /// <summary>
        ///     Sound effect to play when a player grabs on to the platform. Optional if not playing an attaching sound effect.
        /// </summary>
        /// <remarks>Could be swapped for a list of sounds to pick from randomly.</remarks>
        [Tooltip("Sound effect to play when a player grabs on to the platform. Optional if not playing an attaching sound effect.")]
        public AudioClip? attachSFX;

        /// <summary>
        ///     Sound effect to play when the platform despawns and is destroyed. Optional if not playing a despawn sound effect.
        /// </summary>
        /// <remarks>Could be swapped for a list of sounds to pick from randomly.</remarks>
        [Tooltip("Sound effect to play when the platform despawns and is destroyed. Optional if not playing a despawn sound effect.")]
        public AudioClip? destroySFX;

        /// <summary>
        ///     Lowest pitch that platform sound effects can have. Both can be set to the same value to disable pitch variation.
        /// </summary>
        [Tooltip("Lowest pitch that platform sound effects can have. Both can be set to the same value to disable pitch variation.")]
        [Range(-3.0f, 3.0f)]
        public float minPitch = 1.0f;

        /// <summary>
        ///     Highest pitch that platform sound effects can have. Both can be set to the same value to disable pitch variation.
        /// </summary>
        [Tooltip("Highest pitch that platform sound effects can have. Both can be set to the same value to disable pitch variation.")]
        [Range(-3.0f, 3.0f)]
        public float maxPitch = 1.0f;

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

            if (platformAnimator != null)
            {
                platformAnimator.enabled = true;
                platformAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics; // Works much better for moving the player.
            }

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
                attachedPlayerTransform.position = platformTransform.position + playerOffset;

                // Reset attached player's fall time to avoid instant death upon colliding with another (solid) object.
                attachedPlayer.ResetFallGravity();
            }

            base.Update();
        }

        /// <summary>
        ///     Update fields and properties when the object spawns.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Check if platform has an AudioSource set and enabled.
            if (platformSource != null && platformSource.enabled)
            {
                platformSource.pitch = SyncedPitch.Value;

                // Update platform pitch if the network variable is modified.
                SyncedPitch.OnValueChanged += (_, current) => platformSource.pitch = current;
            }

            if (IsHost)
            {
                if (minPitch < maxPitch)
                {
                    // Obtain a random pitch value between configured minimum and maximum values.
                    SyncedPitch.Value = UnityEngine.Random.Range(minPitch, maxPitch);
                }

                if (stateVariant >= 0)
                {
                    // Set animation variant for the platform on the network.
                    SyncedStateVariant.Value = stateVariant;
                }
            }

            if (platformSource != null && spawnSFX != null)
            {
                // Play sound effect when spawning, if one is provided.
                platformSource.PlayOneShot(spawnSFX, 1.0f);
            }
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

        /// <summary>
        ///     Attach player to the platform on the local client.
        /// </summary>
        /// <param name="player">Player to attach to the platform.</param>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            if (attachSFX != null && platformSource != null)
            {
                // Play sound effect immediately after attaching, if one is provided.
                platformSource.PlayOneShot(attachSFX, 1.0f);
            }

            if (stateName.Length > 0 && platformAnimator != null)
            {
                // Play animation immediately after attaching, if one is provided.
                platformAnimator.Play($"{stateName}{(SyncedStateVariant.Value >= 0 ? SyncedStateVariant.Value : "")}");
            }

            base.AttachPlayerLocal(player);
        }

        /// <summary>
        ///     Detach player from the platform on the local client.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (destroySFX != null && platformSource != null)
            {
                // Play sound effect immediately after detaching, if one is provided.
                platformSource.Stop();
                platformSource.PlayOneShot(destroySFX, 1f);
            }

            base.DetachPlayerLocal();
        }

        /// <summary>
        ///     Switch animation variant for the platform locally. Won't actually synchronize with other players unless the platform is spawning.
        /// </summary>
        /// <param name="variant">Value of the animation variant to switch to.</param>
        public void SwitchVariant(int variant)
        {
            stateVariant = variant;
        }
    }
}