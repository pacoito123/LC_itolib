using GameNetcodeStuff;
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
        public int minDamage = 0;

        /// <summary>
        ///     Maximum damage value to inflict on a player.
        /// </summary>
        [Tooltip("Maximum damage value to inflict on a player.")]
        public int maxDamage = 0;

        /// <summary>
        ///     Whether or not to play the vanilla player damage sound effect.
        /// </summary>
        [Header("Audio")]
        [Tooltip("Whether or not to play the vanilla player damage sound effect.")]
        public bool playDamageSFX = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Death")]
        [Tooltip("")]
        public int deathCause = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int deathAnimation = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Vector3 deathLaunchForce = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerHurt"></param>
        public void Ouchie(PlayerControllerB playerHurt)
        {
            if (playerHurt.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId
                || playerHurt.isPlayerDead || !playerHurt.isActiveAndEnabled)
            {
                return;
            }

            int damageTaken = (minDamage < maxDamage) ? Random.Range(minDamage, maxDamage + 1) : minDamage;
            playerHurt.DamagePlayer(damageTaken, playDamageSFX, true, (CauseOfDeath)deathCause,
                deathAnimation, false, deathLaunchForce);
        }
    }
}