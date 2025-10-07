using GameNetcodeStuff;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     Represents a movement hinderance to be applied on to attaching players.
    /// </summary>
    /// <remarks>Intended for stuff like fake water, fake quicksand, fake spider webs, or a general temporary slowing effect.</remarks>
    public class PlayerHinderer : PlayerAttachable
    {
        /// <summary>
        ///     <c>Collider</c> for the hinderance region.
        /// </summary>
        /// <remarks><b>NOTE:</b> Only needed for player drowning purposes.</remarks>
        [Space(10.0f)]
        [Header("Player Hinderer")]
        [Tooltip("Collider for the hinderance region. NOTE: Only needed for player drowning purposes.")]
        [SerializeField] private Collider? hindererCollider;

        /// <summary>
        ///     Whether the player should have slowness applied to them or not.
        /// </summary>
        [Tooltip("Whether the player should have slowness applied to them or not.")]
        [SerializeField] private bool hinderPlayer = true;

        /// <summary>
        ///     Multiplier for the slowness to be applied to the player.
        /// </summary>
        [Tooltip("Multiplier for the slowness to be applied to the player.")]
        [Min(0.01f)]
        [SerializeField] private float hinderedMultiplier = 2.5f;

        /// <summary>
        ///     Whether the vanilla quicksand and underwater sound effects should be muted or not.
        /// </summary>
        [Tooltip("Whether the vanilla quicksand and underwater sound effects should be muted or not.")]
        [SerializeField] private bool muteAudio;

        /// <summary>
        ///     Whether the attached player should be allowed to jump while hindered or not.
        /// </summary>
        [Header("Jumping")]
        [Tooltip("Whether the attached player should be allowed to jump while hindered or not")]
        [SerializeField] private bool allowJumping = true;

        /// <summary>
        ///     Whether stamina is required to be able to jump while hindered or not.
        /// </summary>
        [Tooltip("Whether stamina is required to be able to jump while hindered or not.")]
        [SerializeField] private bool requireStamina;

        /// <summary>
        ///     Whether the hindrance region should act as quicksand or not.
        /// </summary>
        /// <remarks><b>NOTE:</b> Requires a <c>Collider</c> with the <c>Gravel</c> tag to be under the region.</remarks>
        [Header("Quicksand")]
        [Tooltip("Whether the hindrance region should act as quicksand or not. NOTE: Requires a Collider with the 'Gravel' tag to be under the region.")]
        [SerializeField] private bool sinkPlayer;

        /// <summary>
        ///     Multiplier for the speed at which the player sinks.
        /// </summary>
        [Tooltip("Multiplier for the speed at which the player sinks.")]
        [Min(0.0f)]
        [SerializeField] private float sinkingSpeedMultiplier = 0.21f;

        /// <summary>
        ///     <c>AnimationCurve</c> for the vertical distance that the player sinks.
        /// </summary>
        [Tooltip("AnimationCurve for the vertical distance that the player sinks.")]
        [SerializeField] private AnimationCurve? playerSinkingCurveOverride;

        /// <summary>
        ///     Whether the player should be able to drown in the hinderance region or not.
        /// </summary>
        /// <remarks><b>NOTE:</b> Not currently implemented.</remarks>
        [Header("Water")]
        [Tooltip("Whether the player should be able to drown in the hinderance region or not. NOTE: Not currently implemented.")]
        [SerializeField] private bool drownPlayer;

        /// <summary>
        ///     Whether to display the underwater overlay or not.
        /// </summary>
        /// <remarks><b>NOTE:</b> Not currently implemented.</remarks>
        [Tooltip("Whether to display the underwater overlay or not. NOTE: Not currently implemented.")]
        [SerializeField] private bool waterOverlay;

        /// <summary>
        ///     Whether the player should become drunk while hindered or not.
        /// </summary>
        [Header("Drunkness")]
        [Tooltip("Whether the player should become drunk while hindered or not.")]
        [SerializeField] private bool inebriatePlayer;

        /// <summary>
        ///     Speed at which the player should get drunk over time.
        /// </summary>
        [Tooltip("Speed at which the player should get drunk over time.")]
        [Min(0.0f)]
        [SerializeField] private float drunknessSpeed = 0.4f;

        /// <summary>
        ///     Whether the player should have their stamina drained while hindered or not.
        /// </summary>
        [Header("Stamina Drain")]
        [Tooltip("Whether the player should have their stamina drained while hindered or not.")]
        [SerializeField] private bool drainPlayer;

        /// <summary>
        ///     Speed at which the player should have their stamina drained over time.
        /// </summary>
        [Tooltip("Speed at which the player should have their stamina drained over time.")]
        [Min(0.0f)]
        [SerializeField] private float drainSpeed = 1.0f;

        /// <summary>
        ///     Whether the player stamina draining speed should be affected by carry weight or not.
        /// </summary>
        [Tooltip("Whether the player stamina draining speed should be affected by carry weight or not.")]
        [SerializeField] private bool carryWeightAffectsDrain;

        /// <summary>
        ///     Callback invoked when a player begins to be hindered, with the player in question as parameter.
        /// </summary>
        [Header("Events")]
        [Tooltip("Callback invoked when a player begins to be hindered, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onHinderStart = new();

        /// <summary>
        ///     Callback invoked after a player stops being hindered, with the player in question as parameter.
        /// </summary>
        [Tooltip("Callback invoked after a player stops being hindered, with the player in question as parameter.")]
        [SerializeField] private UnityEvent<PlayerControllerB> onHinderStop = new();

        /// <summary>
        ///     Vanilla player sinking <c>AnimationCurve</c>.
        /// </summary>
        private AnimationCurve? defaultPlayerSinkingCurve;

        /// <summary>
        ///     Set player sinking <c>AnimationCurve</c> to (approximately) the vanilla default.
        /// </summary>
        protected override void Reset()
        {
            // Vanilla player sinking curve.
            Keyframe[] defaultSinkingKeyframes = [new(0.0f, 0.0f, 5.4615f, 5.4615f, 0.0f, 0.3333f),
                new(0.0415f, 0.2266f, 0.4576f, 0.4576f, 0.3333f, 0.2344f),
                new(0.3617f, 0.3527f, 0.3546f, 0.3546f, 0.3544f, 0.3333f),
                new(0.7895f, 0.8496f, 1.9803f, 1.9803f, 0.1134f, 0.3333f),
                new(1.0f, 1.0f, 0.3884f, 0.3884f, 0.7641f, 0.0f)];

            playerSinkingCurveOverride = new(defaultSinkingKeyframes);
            // ...

            base.Reset();
        }

        /// <summary>
        ///     Attach if the player is alive.
        ///     Detach if the player is dead.
        /// </summary>
        protected override void Awake()
        {
            attachCondition = player => !player.isPlayerDead;
            detachCondition = player => player.isPlayerDead;
        }

        /// <summary>
        ///     Obtain vanilla player sinking <c>AnimationCurve</c>.
        /// </summary>
        protected override void Start()
        {
            defaultPlayerSinkingCurve = StartOfRound.Instance != null ? StartOfRound.Instance.playerSinkingCurve : null;

            base.Start();
        }

        /// <summary>
        ///     Handle jumping without stamina while hindered.
        /// </summary>
        protected override void Update()
        {
            if (localPlayerAttached && attachedPlayer != null)
            {
                // Check if player should be allowed to jump without stamina.
                if (allowJumping && !requireStamina && attachedPlayer.isExhausted)
                {
                    // Set player as not exhausted, to allow them to jump.
                    attachedPlayer.isExhausted = false;
                }

                // Check if player should become drunk.
                if (inebriatePlayer)
                {
                    // Increase player drunkness inertia.
                    attachedPlayer.drunknessInertia = Mathf.Clamp(attachedPlayer.drunknessInertia + (Time.deltaTime / 1.75f * drunknessSpeed), 0.1f, 3.0f);
                    attachedPlayer.increasingDrunknessThisFrame = true;
                }

                // Check if player should have their stamina drained.
                if (drainPlayer)
                {
                    // Obtain amount to drain from the player's stamina.
                    float drainAmount = Time.deltaTime / attachedPlayer.sprintTime * drainSpeed;

                    // Check if carry weight should affect drain amount.
                    if (carryWeightAffectsDrain)
                    {
                        drainAmount *= attachedPlayer.carryWeight;
                    }

                    // Subtract drain amount from the player's stamina.
                    attachedPlayer.sprintMeter = Mathf.Clamp(attachedPlayer.sprintMeter - drainAmount, 0.0f, 1.0f);
                }

                /* if (drownPlayer)
                {
                    if (hindererCollider?.bounds.Contains(AttachedPlayer.gameplayCamera.transform.position) == true)
                    {
                        
                    }
                } */
            }

            base.Update();
        }

        /// <summary>
        ///     Attach player on the local client.
        /// </summary>
        /// <param name="player">Player to attach.</param>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            // Check if player was attached successfully.
            if (attachedPlayer != null)
            {
                // Hinder player for all clients, unless not spawned or attaching locally.
                HinderPlayer(player, stop: false);
            }

        }

        /// <summary>
        ///     Detach player on the local client.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            // Check if player is attached.
            if (attachedPlayer != null)
            {
                // Stop hindering player for all clients, unless not spawned or attached locally.
                HinderPlayer(attachedPlayer, stop: true);
            }

            base.DetachPlayerLocal();
        }

        /// <summary>
        ///     Hinder given player for all clients, unless not spawned or attaching locally.
        /// </summary>
        /// <param name="player">Player to be hindered.</param>
        /// <param name="stop">Whether to stop hindering the player or not.</param> 
        private void HinderPlayer(PlayerControllerB player, bool stop)
        {
            // Check if hindering the local client.
            if (!player.IsLocalClient())
            {
                return;
            }

            // Hinder player for the local client.
            HinderPlayerLocal(player, stop);

            if (IsSpawned) // TODO: Separate local field?
            {
                // Hinder player for all clients.
                HinderPlayerServerRpc(player, stop);
            }
        }

        /// <summary>
        ///     Hinder given player on the server.
        /// </summary>
        /// <param name="playerReference">Network reference of the hindered player.</param>
        /// <param name="stop">Whether to stop hindering the player or not.</param>
        [ServerRpc(RequireOwnership = false)]
        private void HinderPlayerServerRpc(NetworkBehaviourReference playerReference, bool stop)
        {
            HinderPlayerClientRpc(playerReference, stop);
        }

        /// <summary>
        ///     Hinder given player on all clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the hindered player.</param>
        /// <param name="stop">Whether to stop hindering the player or not.</param>
        [ClientRpc]
        private void HinderPlayerClientRpc(NetworkBehaviourReference playerReference, bool stop)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                HinderPlayerLocal(player, stop);
            }
        }

        /// <summary>
        ///     Hinder given player on the local client.
        /// </summary>
        /// <param name="player">Player to be hindered.</param>
        /// <param name="stop">Whether to stop hindering the player or not.</param> 
        private void HinderPlayerLocal(PlayerControllerB player, bool stop)
        {
            if (!stop)
            {
                // Check if player should be hindered.
                if (hinderPlayer)
                {
                    // Add hindrance source and multiplier.
                    player.isMovementHindered++;
                    player.hinderedMultiplier *= hinderedMultiplier;
                }

                // Enable or disable player status effect audio.
                player.statusEffectAudio.enabled = !muteAudio;

                // Check if player should be able to sink.
                if (sinkPlayer)
                {
                    if (player.IsLocalClient() && playerSinkingCurveOverride != null && StartOfRound.Instance != null)
                    {
                        // Override vanilla player sinking curve while attached.
                        StartOfRound.Instance.playerSinkingCurve = playerSinkingCurveOverride;
                    }

                    // Add sinking source and apply sinking speed multiplier.
                    player.sourcesCausingSinking++;
                    player.sinkingSpeedMultiplier = sinkingSpeedMultiplier;
                }
                else if (drownPlayer || (hinderPlayer && allowJumping))
                {
                    // Set player as being underwater.
                    player.isUnderwater = true; // Also needed to allow the player to jump while hindered.

                    // Check if player should be able to drown.
                    if (drownPlayer)
                    {
                        // Set player underwater collider to the specified collider.
                        player.underwaterCollider = hindererCollider;

                        // Check if water overlay should be displayed.
                        if (waterOverlay)
                        {

                        }
                    }
                }

                // Invoke hinder begin event.
                onHinderStart.Invoke(player);
            }
            else
            {
                // Check if player was hindered.
                if (hinderPlayer)
                {
                    // Remove hindrance source and multiplier.
                    player.isMovementHindered--;
                    player.hinderedMultiplier /= hinderedMultiplier;
                }

                // Reenable player status effect audio.
                player.statusEffectAudio.enabled = true;

                if (sinkPlayer)
                {
                    if (player.IsLocalClient() && playerSinkingCurveOverride != null && StartOfRound.Instance != null)
                    {
                        // Restore vanilla player sinking curve after detaching.
                        StartOfRound.Instance.playerSinkingCurve = defaultPlayerSinkingCurve;
                    }

                    // Remove sinking source and reset sinking speed multiplier.
                    player.sourcesCausingSinking--;
                    player.sinkingSpeedMultiplier = 0.0f;
                }
                else if (drownPlayer || (hinderPlayer && allowJumping))
                {
                    // Set player as no longer underwater.
                    player.isUnderwater = false;

                    if (drownPlayer)
                    {
                        // Remove player underwater collider.
                        player.underwaterCollider = null;
                    }
                }

                // Invoke hinder stop event.
                onHinderStop.Invoke(player);
            }
        }

        /// <summary>
        ///     Drains the attached player... <i>Ayo</i>?
        /// </summary>
        /// <param name="drainAmount">Amount to drain from the attached player.</param>
        public void DrainPlayer(float drainAmount)
        {
            // Check if the local player is not attached, or is already exhausted.
            if (!localPlayerAttached || attachedPlayer == null || attachedPlayer.isExhausted)
            {
                return;
            }

            // Subtract drain amount from the player's stamina.
            attachedPlayer.sprintMeter = Mathf.Clamp(attachedPlayer.sprintMeter - drainAmount, 0.0f, 1.0f);
        }
    }
}