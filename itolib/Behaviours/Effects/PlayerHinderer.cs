using GameNetcodeStuff;
using itolib.Behaviours.Networking;
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
        [Header("Water")]
        [Tooltip("Whether the player should be able to drown in the hinderance region or not.")]
        [SerializeField] private bool drownPlayer;

        /// <summary>
        ///     Whether to display the underwater <c>Volume</c> overlay or not.
        /// </summary>
        [Tooltip("Whether to display the underwater Volume overlay or not.")]
        [SerializeField] private bool setUnderwaterVolumeFilter = true;

        /// <summary>
        ///     Whether to apply underwater audio occlusion or not.
        /// </summary>
        [Tooltip("Whether to apply underwater audio occlusion or not.")]
        [SerializeField] private bool setUnderwaterOcclusionFilter = true;

        /// <summary>
        ///     Minimum fall length required to play the small water splash particle effect. Can be set to <c>0</c> to disable it.
        /// </summary>
        [Tooltip("Minimum fall length required to play the small water splash particle effect. Can be set to '0' to disable it.")]
        [Range(0.0f, 150.0f)]
        [SerializeField] private float waterSplashSmallFallValue = 4.0f;

        /// <summary>
        ///     Small water splash particle effect to play when jumping into the hinderance region.
        /// </summary>
        [Tooltip("Small water splash particle effect to play when jumping into the hinderance region.")]
        [SerializeField] private ParticleSystem? waterSplashSmallOverride;

        /// <summary>
        ///     Small water splash sound effects to play when jumping into the hinderance region.
        /// </summary>
        [Tooltip("Small water splash sound effects to play when jumping into the hinderance region.")]
        [SerializeField] private AudioClip[]? waterSplashSmallSFXOverride;

        /// <summary>
        ///     Minimum fall length required to play the big water splash particle effect. Can be set to <c>0</c> to disable it.
        /// </summary>
        [Tooltip("Minimum fall length required to play the big water splash particle effect. Can be set to '0' to disable it.")]
        [Range(0.0f, 150.0f)]
        [SerializeField] private float waterSplashBigFallValue = 17.0f;

        /// <summary>
        ///     Big water splash particle effect to play when jumping into the hinderance region.
        /// </summary>
        [Tooltip("Big water splash particle effect to play when jumping into the hinderance region.")]
        [SerializeField] private ParticleSystem? waterSplashBigOverride;

        /// <summary>
        ///     Big water splash sound effects to play when jumping into the hinderance region.
        /// </summary>
        [Tooltip("Big water splash sound effects to play when jumping into the hinderance region.")]
        [SerializeField] private AudioClip[]? waterSplashBigSFXOverride;

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
        private static AnimationCurve? defaultPlayerSinkingCurve;

        /// <summary>
        ///     Vanilla water splashing <c>ParticleSystem</c> instances.
        /// </summary>
        private static ParticleSystem? defaultWaterSplashSmall, defaultWaterSplashBig;

        /// <summary>
        ///     Vanilla water splashing <c>AudioClip</c> lists.
        /// </summary>
        private static AudioClip[]? defaultWaterSplashSmallSFX, defaultWaterSplashBigSFX;

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
        ///     Attach if the player is alive and enabled.
        /// </summary>
        /// <param name="player">Player to check for attaching.</param>
        /// <returns>Whether the player should attach or not.</returns>
        protected override bool AttachCondition(PlayerControllerB player)
        {
            return !player.isPlayerDead && player.isActiveAndEnabled;
        }

        /// <summary>
        ///     Detach if the player is dead or disabled.
        /// </summary>
        /// <param name="player">Player to check for detaching.</param>
        /// <returns>Whether the player should detach or not.</returns>
        protected override bool DetachCondition(PlayerControllerB player)
        {
            return player.isPlayerDead || !player.isActiveAndEnabled;
        }

        /// <summary>
        ///     Obtain default vanilla values for resetting overrides.
        /// </summary>
        protected override void Start()
        {
            defaultPlayerSinkingCurve = StartOfRound.Instance != null ? StartOfRound.Instance.playerSinkingCurve : null;

            if (TimeOfDay.Instance != null)
            {
                defaultWaterSplashSmall = TimeOfDay.Instance.waterSplashParticleSmall;
                defaultWaterSplashBig = TimeOfDay.Instance.waterSplashParticle;
            }

            if (SoundManager.Instance != null)
            {
                defaultWaterSplashSmallSFX = SoundManager.Instance.waterSplashSFXSmall;
                defaultWaterSplashBigSFX = SoundManager.Instance.waterSplashSFX;
            }

            if (waterSplashSmallFallValue > waterSplashBigFallValue && waterSplashBigFallValue != 0.0f)
            {
                waterSplashSmallFallValue = waterSplashBigFallValue;
            }

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

                if (!drownPlayer && attachedPlayer.isUnderwater && StartOfRound.Instance != null)
                {
                    // Reset drowning timer, if set to NOT drown the player.
                    StartOfRound.Instance.drowningTimer = 1.0f;

                    if (muteAudio)
                    {
                        // Mute audio effects for this frame.
                        StartOfRound.Instance.playedDrowningSFX = true;
                    }
                }
            }

            base.Update();
        }

        /// <summary>
        ///     Handle some underwater overrides.
        /// </summary>
        private void LateUpdate()
        {
            if (localPlayerAttached && attachedPlayer != null)
            {
                if (HUDManager.Instance != null && HUDManager.Instance.setUnderwaterFilter)
                {
                    if (!setUnderwaterVolumeFilter)
                    {
                        // Disable underwater screen filter.
                        HUDManager.Instance.setUnderwaterFilter = false;
                        HUDManager.Instance.underwaterScreenFilter.weight = 0.0f;
                    }

                    if (!setUnderwaterOcclusionFilter)
                    {
                        // Disable underwater audio occlusion.
                        HUDManager.Instance.audioListenerLowPass.enabled = false;
                        HUDManager.Instance.audioListenerLowPass.cutoffFrequency = 20000.0f;
                    }
                }
            }
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
                // Hinder player for all other clients.
                HinderPlayerRpc(player, stop);
            }
        }

        /// <summary>
        ///     Hinder given player on all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the hindered player.</param>
        /// <param name="stop">Whether to stop hindering the player or not.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void HinderPlayerRpc(NetworkBehaviourReference playerReference, bool stop)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Hinder player for the local client.
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

                // Check if hinderer audio effects should be muted.
                if (muteAudio)
                {
                    if (player.statusEffectAudio != null)
                    {
                        player.statusEffectAudio.enabled = false;
                    }

                    if (player.IsLocalClient() && HUDManager.Instance != null && HUDManager.Instance.breathingUnderwaterAudio != null)
                    {
                        HUDManager.Instance.breathingUnderwaterAudio.enabled = false;
                    }
                }

                // Check if player should be able to sink.
                if (sinkPlayer)
                {
                    if (player.IsLocalClient() && playerSinkingCurveOverride != null && StartOfRound.Instance != null)
                    {
                        // Override vanilla player sinking curve while attached.
                        StartOfRound.Instance.playerSinkingCurve = playerSinkingCurveOverride;
                    }

                    // Set quicksand status effect audio.
                    player.statusEffectAudioIndex = 0;

                    // Add sinking source and apply sinking speed multiplier.
                    player.sourcesCausingSinking++;
                    player.sinkingSpeedMultiplier = sinkingSpeedMultiplier;
                }
                else if (drownPlayer || (hinderPlayer && allowJumping))
                {
                    // Set water status effect audio.
                    player.statusEffectAudioIndex = 1;

                    // Set player as being underwater.
                    player.isUnderwater = true; // Also needed to allow the player to jump while hindered.

                    // Set player underwater collider to the specified collider.
                    player.underwaterCollider = hindererCollider;
                }

                if (hindererCollider != null && (player.isFallingFromJump || player.isFallingNoJump)
                    && ((waterSplashSmallFallValue != 0.0f && player.fallValue < -waterSplashSmallFallValue)
                    || (waterSplashBigFallValue != 0.0f && player.fallValue < -waterSplashBigFallValue)))
                {
                    PlayWaterSplash(player, waterSplashBigFallValue == 0.0f || player.fallValue >= -waterSplashBigFallValue);
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

                // Reenable hinderer audio effects.
                if (muteAudio)
                {
                    if (player.statusEffectAudio != null)
                    {
                        player.statusEffectAudio.enabled = true;
                    }

                    if (player.IsLocalClient() && HUDManager.Instance != null && HUDManager.Instance.breathingUnderwaterAudio != null)
                    {
                        HUDManager.Instance.breathingUnderwaterAudio.enabled = true;
                    }
                }

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

                    // Remove player underwater collider.
                    player.underwaterCollider = null;
                }

                // Invoke hinder stop event.
                onHinderStop.Invoke(player);
            }
        }

        /// <summary>
        ///     Play water splash effect at a player's location.
        /// </summary>
        /// <param name="player">Player doing the splashing.</param>
        /// <param name="smallSplash">Whether the splash is small or big.</param>
        private void PlayWaterSplash(PlayerControllerB player, bool smallSplash)
        {
            // Play water splash effect for the local client.
            PlayWaterSplashLocal(player, smallSplash);

            if (IsSpawned) // TODO: Separate local field?
            {
                // Play water splash effect for all other clients.
                PlayWaterSplashRpc(player, smallSplash);
            }
        }

        /// <summary>
        ///     Play water splash effect at a player's location on all other clients.
        /// </summary>
        /// <param name="playerReference">Network reference of the player doing the splashing.</param>
        /// <param name="smallSplash">Whether the splash is small or big.</param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void PlayWaterSplashRpc(NetworkBehaviourReference playerReference, bool smallSplash)
        {
            if (playerReference.TryGet(out PlayerControllerB player))
            {
                // Play water splash effect for the local client.
                PlayWaterSplashLocal(player, smallSplash);
            }
        }

        /// <summary>
        ///     Play water splash effect at a player's location for the local client.
        /// </summary>
        /// <param name="player">Player doing the splashing.</param>
        /// <param name="smallSplash">Whether the splash is small or big.</param>
        private void PlayWaterSplashLocal(PlayerControllerB player, bool smallSplash)
        {
            // Apply any water effect overrides before splashing.
            ApplyWaterParticleOverrides();

            if (TimeOfDay.Instance != null)
            {
                // Splash!
                TimeOfDay.Instance.WaterSplashEffect(player.transform.position, smallSplash);
            }

            // Reset any water effect overrides after splashing.
            ApplyWaterParticleOverrides(undo: true);
        }

        /// <summary>
        ///     Apply water splashing effect overrides, or reset them.
        /// </summary>
        /// <param name="undo">Whether water splashing effect overrides should be reset or not.</param>
        private void ApplyWaterParticleOverrides(bool undo = false)
        {
            if (TimeOfDay.Instance != null)
            {
                if (waterSplashSmallOverride != null)
                {
                    // Override small splash particle effect.
                    TimeOfDay.Instance.waterSplashParticleSmall = (!undo) ? waterSplashSmallOverride : defaultWaterSplashSmall;
                }

                if (waterSplashBigOverride != null)
                {
                    // Override big splash particle effect.
                    TimeOfDay.Instance.waterSplashParticle = (!undo) ? waterSplashBigOverride : defaultWaterSplashBig;
                }
            }

            if (SoundManager.Instance != null)
            {
                if (waterSplashSmallSFXOverride?.Length > 0)
                {
                    // Override small splash sound effects.
                    SoundManager.Instance.waterSplashSFXSmall = (!undo) ? waterSplashSmallSFXOverride : defaultWaterSplashSmallSFX;
                }

                if (waterSplashBigSFXOverride?.Length > 0)
                {
                    // Override big splash sound effects.
                    SoundManager.Instance.waterSplashSFX = (!undo) ? waterSplashBigSFXOverride : defaultWaterSplashBigSFX;
                }
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