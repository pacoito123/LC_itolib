using GameNetcodeStuff;
using UnityEngine;

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
        [Tooltip("")]
        public bool allowJumping = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            AttachCondition = player => !player.isPlayerDead;
            DetachCondition = player => player.isPlayerDead;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void AttachPlayerLocal(PlayerControllerB player)
        {
            base.AttachPlayerLocal(player);

            player.isMovementHindered++;
            player.hinderedMultiplier *= hinderedMultiplier;

            if (allowJumping)
            {
                player.isUnderwater = true;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void DetachPlayerLocal()
        {
            if (AttachedPlayer != null)
            {
                AttachedPlayer.isMovementHindered--;
                AttachedPlayer.hinderedMultiplier /= hinderedMultiplier;

                if (allowJumping)
                {
                    AttachedPlayer.isUnderwater = false;
                }
            }

            base.DetachPlayerLocal();
        }
    }
}