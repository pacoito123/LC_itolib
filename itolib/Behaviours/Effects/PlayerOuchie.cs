using GameNetcodeStuff;
using itolib.Extensions;
using UnityEngine;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    /// 	Script to deal a set (or random) amount of damage to a given player.
    /// </summary>
    public class PlayerOuchie : MonoBehaviour
    {
        /// <summary>
        ///     Minimum damage value to inflict on a player.
        /// </summary>
        [Tooltip("Minimum damage value to inflict on a player.")]
        [SerializeField] private int minDamage;

        /// <summary>
        ///     Maximum damage value to inflict on a player.
        /// </summary>
        [Tooltip("Maximum damage value to inflict on a player.")]
        [SerializeField] private int maxDamage;

        /// <summary>
        ///     Whether or not to play the vanilla player damage sound effect.
        /// </summary>
        [Header("Audio")]
        [Tooltip("Whether or not to play the vanilla player damage sound effect.")]
        [SerializeField] private bool playDamageSFX = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Death")]
        [Tooltip("")]
        [SerializeField] private CauseOfDeath deathCause = CauseOfDeath.Unknown;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private int deathAnimation;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Vector3 deathLaunchForce = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerHurt"></param>
        public void Ouchie(PlayerControllerB playerHurt)
        {
            if (!playerHurt.IsLocalClient() || playerHurt.isPlayerDead || !playerHurt.isActiveAndEnabled)
            {
                return;
            }

            int damageTaken = Random.Range(minDamage, maxDamage + 1);
            playerHurt.DamagePlayer(damageTaken, playDamageSFX, true, deathCause,
                deathAnimation, false, deathLaunchForce);
        }
    }
}