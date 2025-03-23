using GameNetcodeStuff;
using UnityEngine;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    /// 	TODO.
    /// </summary>
    public class PlayerOuchie : MonoBehaviour
    {
        /// <summary>
        ///     Time-seeded (local) Random instance for randomization purposes.
        /// </summary>
        public static System.Random Random { get; private set; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int minDamage = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int maxDamage = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Audio")]
        [Tooltip("")]
        public AudioSource? ouchSource;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AudioClip? ouchSfx;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float ouchVolume = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Launch")]
        [Tooltip("")]
        public Vector3 launchForce = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool dealFallDamage = false;

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
            if (ouchSfx != null)
            {
                ouchSource?.PlayOneShot(ouchSfx, ouchVolume);
            }

            if (playerHurt.actualClientId != GameNetworkManager.Instance.localPlayerController.actualClientId
                || playerHurt.isPlayerDead || !playerHurt.isActiveAndEnabled)
            {
                return;
            }

            int damageTaken = minDamage != maxDamage ? Random.Next(minDamage, maxDamage + 1) : minDamage;
            playerHurt.DamagePlayer(damageTaken, ouchSfx == null, true, (CauseOfDeath)deathCause,
                deathAnimation, dealFallDamage, deathLaunchForce);

            playerHurt.externalForceAutoFade = launchForce;
        }
    }
}