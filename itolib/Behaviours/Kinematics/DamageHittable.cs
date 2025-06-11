using GameNetcodeStuff;
using itolib.Extensions;
using itolib.Behaviours.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Kinematics
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [Serializable]
    public struct HealthCondition
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int healthAmount;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onReachedHealth;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onBelowHealth;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onAboveHealth;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool runOnce;

        /// <summary>
        ///     TODO.
        /// </summary>
        public HealthCondition()
        {
            onReachedHealth = new();
            onBelowHealth = new();
            onAboveHealth = new();
            runOnce = true;
        }
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
        public int health;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public List<HealthCondition> healthConditions = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            healthConditions = [.. healthConditions.OrderByDescending(condition => condition.healthAmount)];
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="hitInfo"></param>
        public override void PerformHitLocal(HitInfo hitInfo)
        {
            onHit.Invoke();

            if (health == 0)
            {
                return;
            }

            health = Mathf.Clamp(health - hitInfo.damage, 0, health);

            for (int i = 0; i < healthConditions.Count; i++)
            {
                if (health > healthConditions[i].healthAmount)
                {
                    break;
                }
                else
                {
                    if (health == healthConditions[i].healthAmount)
                    {
                        healthConditions[i].onReachedHealth.Invoke(health);
                    }

                    if (health < healthConditions[i].healthAmount)
                    {
                        healthConditions[i].onBelowHealth.Invoke(health);
                    }

                    if (health > healthConditions[i].healthAmount)
                    {
                        healthConditions[i].onAboveHealth.Invoke(health);
                    }
                }
            }

            _ = healthConditions.RemoveAll(condition => health <= condition.healthAmount && condition.runOnce);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="health"></param>
        public void SetHealth(int health)
        {
            SetHealthLocal(health);
            SetHealthServerRpc(GameNetworkManager.Instance.localPlayerController, health);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="health"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SetHealthServerRpc(NetworkBehaviourReference playerReference, int health)
        {
            SetHealthClientRpc(playerReference, health);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="health"></param>
        [ClientRpc]
        public void SetHealthClientRpc(NetworkBehaviourReference playerReference, int health)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                SetHealthLocal(health);
            }
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