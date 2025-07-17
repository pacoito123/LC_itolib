using GameNetcodeStuff;
using itolib.Extensions;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct SprayThreshold
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Spray Threshold")]
        [Tooltip("")]
        [Min(1)]
        public int spraysRequired = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onReachedThreshold = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onBelowThreshold = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onAboveThreshold = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool triggerOnce = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        public SprayThreshold() { }
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class SpraySensor : MovementSensor
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Spray Sensor")]
        [Tooltip("")]
        [SerializeField] private List<SprayThreshold> sprayThresholds = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool allowSprayPaint = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool allowWeedKiller = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Range(0.0f, 180.0f)]
        [SerializeField] private float sprayAngle = 45.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private int sprayRange = 2;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<PlayerControllerB> onSprayPerformed = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private int timesSprayed;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Reset()
        {
            attachLocally = true;
            detachOnExit = true;

            actionToTrigger = "ActivateItem";
            holdAction = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Start()
        {
            sprayThresholds.Sort((thresholdA, thresholdB) => thresholdB.spraysRequired - thresholdA.spraysRequired);

            onMovementDetected.AddListener(PerformSpray);

            base.Start();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="player"></param>
        private void PerformSpray(PlayerControllerB player)
        {
            if (player == null || !player.IsLocalClient())
            {
                return;
            }

            if (player.throwingObject || !player.isHoldingObject || player.currentlyHeldObjectServer == null)
            {
                return;
            }

            if (player.currentlyHeldObjectServer is not SprayPaintItem spray
                || (!allowSprayPaint && !spray.isWeedKillerSprayBottle) || (!allowWeedKiller && spray.isWeedKillerSprayBottle))
            {
                return;
            }

            if (spray.sprayCanTank > 0.0f && spray.sprayCanShakeMeter > 0.0f)
            {
                if (player.HasLineOfSightToPosition(transform.position, sprayAngle, sprayRange, -1))
                {
                    PerformSprayLocal(player);
                    PerformSprayServerRpc(player);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ServerRpc(RequireOwnership = false)]
        private void PerformSprayServerRpc(NetworkBehaviourReference playerReference)
        {
            PerformSprayClientRpc(playerReference);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        [ClientRpc]
        private void PerformSprayClientRpc(NetworkBehaviourReference playerReference)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                PerformSprayLocal(player);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void PerformSprayLocal(PlayerControllerB player)
        {
            timesSprayed++;

            onSprayPerformed.Invoke(player);

            foreach (SprayThreshold sprayThreshold in sprayThresholds)
            {
                if (timesSprayed < sprayThreshold.spraysRequired)
                {
                    sprayThreshold.onBelowThreshold.Invoke(timesSprayed);
                }

                if (timesSprayed == sprayThreshold.spraysRequired)
                {
                    sprayThreshold.onReachedThreshold.Invoke(timesSprayed);
                }

                if (timesSprayed > sprayThreshold.spraysRequired)
                {
                    sprayThreshold.onAboveThreshold.Invoke(timesSprayed);
                }
            }

            _ = sprayThresholds.RemoveAll(sprayThreshold => sprayThreshold.triggerOnce && timesSprayed >= sprayThreshold.spraysRequired);
        }
    }
}