using itolib.Behaviours.Networking;
using itolib.Structs;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct HealthCondition()
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int healthAmount = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onReachedHealth = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onBelowHealth = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onAboveHealth = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool runOnce = true;
    }

    /// <summary>
    ///     TODO.
    /// </summary>
    public class DamageHittable : NetworkedHittable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Damage Hittable")]
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private int health = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private List<HealthCondition> healthConditions = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            healthConditions.Sort((conditionA, conditionB) => conditionB.healthAmount - conditionA.healthAmount);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitInfo"></param>
        public override void PerformHitLocal(HitInfo hitInfo)
        {
            if (health == 0)
            {
                return;
            }

            base.PerformHitLocal(hitInfo);

            health = Mathf.Clamp(health - hitInfo.damage, 0, health);

            foreach (HealthCondition healthCondition in healthConditions)
            {
                if (health > healthCondition.healthAmount)
                {
                    healthCondition.onAboveHealth.Invoke(health);
                }

                if (health == healthCondition.healthAmount)
                {
                    healthCondition.onReachedHealth.Invoke(health);
                }

                if (health < healthCondition.healthAmount)
                {
                    healthCondition.onBelowHealth.Invoke(health);
                }
            }

            _ = healthConditions.RemoveAll(healthCondition => healthCondition.runOnce && health <= healthCondition.healthAmount);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="health"></param>
        public void IncrementHealth(int health)
        {
            SetHealth(this.health + health);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="health"></param>
        public void SetHealth(int health)
        {
            SetHealthLocal(health);

            if (IsSpawned)
            {
                SetHealthRpc(health);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="health"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        public void SetHealthRpc(int health)
        {
            SetHealthLocal(health);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="health"></param>
        public void SetHealthLocal(int health)
        {
            this.health = health;
        }
    }
}