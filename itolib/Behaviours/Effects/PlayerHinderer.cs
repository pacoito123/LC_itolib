using GameNetcodeStuff;
using itolib.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class PlayerHinderer : PlayerAttachable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Player Hinderer")]
        [Tooltip("")]
        public float hinderedMultiplier = 2.5f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Jumping")]
        [Tooltip("")]
        public bool allowJumping = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool requireStamina = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Quicksand")]
        [Tooltip("")]
        public bool sinkPlayer = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float sinkingSpeedMultiplier = 0.21f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AnimationCurve? playerSinkingCurveOverride;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Water")]
        [Tooltip("")]
        public bool drownPlayer = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool waterOverlay = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onHinderStart = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<PlayerControllerB> onHinderStop = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public Collider? hindererCollider;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public AnimationCurve? defaultPlayerSinkingCurve;

        /// <summary>
        ///     TODO.
        /// </summary> 
        public void Reset()
        {
            // This don't seem to work...
            Keyframe[] defaultSinkingKeyframes = [new(0.0f, 0.0f, 5.4615f, 5.4615f, 0.0f, 0.3333f),
                new(0.0415f, 0.2266f, 0.4576f, 0.4576f, 0.3333f, 0.2344f),
                new(0.3617f, 0.3527f, 0.3546f, 0.3546f, 0.3544f, 0.3333f),
                new(0.7895f, 0.8496f, 1.9803f, 1.9803f, 0.1134f, 0.3333f),
                new(1.0f, 1.0f, 0.3884f, 0.3884f, 0.7641f, 0.0f)];

            playerSinkingCurveOverride = new(defaultSinkingKeyframes);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            AttachCondition = player => !player.isPlayerDead;
            DetachCondition = player => player.isPlayerDead;

            if (!TryGetComponent(out hindererCollider))
            {
                // Plugin.StaticLogger.LogWarning(""); // TODO: Warn collider is missing.
            }

            defaultPlayerSinkingCurve = StartOfRound.Instance?.playerSinkingCurve;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Update()
        {
            if (LocalPlayerAttached && AttachedPlayer != null)
            {
                if (allowJumping && requireStamina && AttachedPlayer.isExhausted)
                {
                    AttachedPlayer.isExhausted = false;
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
        ///     TODO.
        /// </summary>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            player.isMovementHindered++;
            player.hinderedMultiplier *= hinderedMultiplier;

            if (sinkPlayer)
            {
                if (playerSinkingCurveOverride != null && StartOfRound.Instance != null)
                {
                    StartOfRound.Instance.playerSinkingCurve = playerSinkingCurveOverride;
                }

                player.sourcesCausingSinking++;
                player.sinkingSpeedMultiplier = sinkingSpeedMultiplier;
            }
            else if (drownPlayer || allowJumping)
            {
                player.isUnderwater = true;

                if (drownPlayer)
                {
                    player.underwaterCollider = hindererCollider;
                }
            }

            onHinderStart.Invoke(player);

            if (!attachLocally)
            {
                HinderPlayerServerRpc(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (AttachedPlayer == null)
            {
                return;
            }

            AttachedPlayer.isMovementHindered--;
            AttachedPlayer.hinderedMultiplier /= hinderedMultiplier;

            if (sinkPlayer)
            {
                if (playerSinkingCurveOverride != null && StartOfRound.Instance != null)
                {
                    StartOfRound.Instance.playerSinkingCurve = defaultPlayerSinkingCurve;
                }

                AttachedPlayer.sourcesCausingSinking--;
                AttachedPlayer.sinkingSpeedMultiplier = 0.0f;
            }
            else if (drownPlayer || allowJumping)
            {
                AttachedPlayer.isUnderwater = false;

                if (drownPlayer)
                {
                    AttachedPlayer.underwaterCollider = null;
                }
            }

            onHinderStop.Invoke(AttachedPlayer);

            if (!attachLocally)
            {
                HinderPlayerServerRpc(AttachedPlayer, stop: true);
            }

            base.DetachPlayerLocal();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="stop"></param>
        [ServerRpc(RequireOwnership = false)]
        public void HinderPlayerServerRpc(NetworkBehaviourReference playerReference, bool stop = false)
        {
            HinderPlayerClientRpc(playerReference, stop);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="stop"></param>
        [ClientRpc]
        public void HinderPlayerClientRpc(NetworkBehaviourReference playerReference, bool stop = false)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                if (!stop)
                {
                    onHinderStart.Invoke(player);
                }
                else
                {
                    onHinderStop.Invoke(player);
                }
            }
        }
    }
}