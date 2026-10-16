using GameNetcodeStuff;
using itolib.Behaviours.Detectors;
using itolib.Util;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     Represents an explosion that can damage, ignite, or kill objects within its range.
    /// </summary>
    public class ExplodeEffect : DetectRegion<NetworkBehaviour>
    {
        /// <summary>
        ///     Explosion prefab to spawn upon triggering.
        /// </summary>
        /// <remarks><b>NOTE:</b> Can be left blank to spawn the vanilla explosion effect.</remarks>
        [Header("Explode Effect")]
        [Tooltip("Explosion prefab to spawn upon triggering. NOTE: Can be left blank to spawn the vanilla explosion effect.")]
        [SerializeField] private GameObject? explosionPrefab;

        /// <summary>
        ///     Whether to spawn an explosion effect upon triggering or not.
        /// </summary>
        [Tooltip("Whether to spawn an explosion effect upon triggering or not.")]
        [SerializeField] private bool spawnExplosionEffect = true;

        /// <summary>
        ///     Distance-based damage to deal to players found within the region.
        /// </summary>
        /// <remarks><c>0</c> is as close to the explosion as possible, <c>1</c> is at maximum range from it.</remarks>
        [Header("Damage")]
        [Tooltip("Distance-based damage to deal to players found within the region. '0' is as close to the explosion as possible, '1' is at maximum range from it.")]
        [SerializeField] private AnimationCurve damageCurve = AnimationCurve.Constant(0.0f, 1.0f, 50.0f);

        /// <summary>
        ///     Distance-based damage to deal to enemies found within the region.
        /// </summary>
        /// <remarks><c>0</c> is as close to the explosion as possible, <c>1</c> is at maximum range from it.</remarks>
        [Tooltip("Distance-based damage to deal to enemies found within the region. '0' is as close to the explosion as possible, '1' is at maximum range from it.")]
        [SerializeField] private AnimationCurve enemyDamageCurve = AnimationCurve.Constant(0.0f, 1.0f, 6.0f);

        /// <summary>
        ///     Distance-based damage to deal to other <c>IHittable</c> objects found within the region.
        /// </summary>
        /// <remarks><c>0</c> is as close to the explosion as possible, <c>1</c> is at maximum range from it.</remarks>
        [Tooltip("Distance-based damage to deal to other hittable objects found within the region. '0' is as close to the explosion as possible, '1' is at maximum range from it.")]
        [SerializeField] private AnimationCurve otherDamageCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

        /// <summary>
        ///     <c>Collider</c> within which players will be instantly killed.
        /// </summary>
        /// <remarks><b>NOTE:</b> Can be left blank to disable explosion insta-kills.</remarks>
        [Tooltip("Collider within which players will be instantly killed. NOTE: Can be left blank to disable explosion insta-kills.")]
        [SerializeField] private Collider? killBounds;

        /// <summary>
        ///     Enemy damaging rules for the explosion.
        /// </summary>
        [Tooltip("Enemy damaging rules for the explosion.")]
        [SerializeField] private EnemyFilter[]? enemyFilters;

        /// <summary>
        ///     Minimum distance required to activate <c>Small</c> camera shaking.
        /// </summary>
        [Header("Camera Shake")]
        [Tooltip("Minimum distance required to activate 'Small' camera shaking.")]
        [Min(0.0f)]
        [SerializeField] private float smallCameraShakeDistance = 25.0f;

        /// <summary>
        ///     Minimum distance required to activate <c>Big</c> camera shaking.
        /// </summary>
        [Tooltip("Minimum distance required to activate 'Big' camera shaking.")]
        [Min(0.0f)]
        [SerializeField] private float bigCameraShakeDistance = 14.0f;

        /// <summary>
        ///     Minimum distance required to activate <c>Long</c> camera shaking.
        /// </summary>
        [Tooltip("Minimum distance required to activate 'Long' camera shaking.")]
        [Min(0.0f)]
        [SerializeField] private float longCameraShakeDistance = 0.0f;

        /// <summary>
        ///     Minimum distance required to activate <c>VeryStrong</c> camera shaking.
        /// </summary>
        [Tooltip("Minimum distance required to activate 'VeryStrong' camera shaking.")]
        [Min(0.0f)]
        [SerializeField] private float veryStrongCameraShakeDistance = 0.0f;

        /// <summary>
        ///     Parent <c>NetworkObject</c> to attempt to despawn upon triggering.
        /// </summary>
        [Header("Despawn")]
        [Tooltip("Parent NetworkObject to attempt to despawn upon triggering.")]
        [SerializeField] private NetworkObject? parentNetworkObject;

        /// <summary>
        ///     Delay in seconds until despawning, to allow effects to play.
        /// </summary>
        [Tooltip("Delay in seconds until despawning, to allow effects to play.")]
        [SerializeField] private float despawnTimer = 0.0f;

        /// <summary>
        ///     <c>LayerMask</c> for all layers that should block damage from the explosion.
        /// </summary>
        [Space(10.0f)]
        [Header("Layer Mask")]
        [Tooltip("LayerMask for all layers that should block damage from the explosion.")]
        [SerializeField] private LayerMask coverMask;

        /// <summary>
        ///     Whether vanilla explosion effect is being used or not, to properly rotate it.
        /// </summary>
        private bool useVanillaExplosion;

        /// <inheritdoc/>
        protected override void Reset()
        {
            ActivationTime = Enums.ActivationTime.Manual;
            maxObjects = 32;

            layerMask = LayerMask.GetMask("Player", "Enemies", "MapHazards");
            coverMask = LayerMask.GetMask("Room", "Vehicle");

            enemyFilters = [new() { enemyName = "Any", fuzzySearch = false, subtractOnExit = false }];

            base.Reset();
        }

        /// <inheritdoc/>
        protected override void Awake()
        {
            if (!NetworkManager.Singleton.IsHost && regionCollider != null)
            {
                regionCollider.enabled = false;
            }

            if (explosionPrefab == null)
            {
                explosionPrefab = (StartOfRound.Instance != null) ? StartOfRound.Instance.explosionPrefab : null;
                useVanillaExplosion = true;
            }
        }

        /// <inheritdoc/>
		public override void CheckObjectsInRegion()
        {
            if (regionCollider == null)
            {
                return;
            }

            Vector3 explosionOrigin = regionCollider.bounds.center;
            if (spawnExplosionEffect && explosionPrefab != null)
            {
                Instantiate(explosionPrefab, explosionOrigin, useVanillaExplosion ? Quaternion.Euler(-90f, 0f, 0f)
                    : Quaternion.identity, (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null)
                        ? RoundManager.Instance.mapPropsContainer.transform : null).SetActive(true);
            }

            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || HUDManager.Instance == null)
            {
                return;
            }

            float shakeDistance = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionOrigin);

            if (smallCameraShakeDistance > 0 && shakeDistance < smallCameraShakeDistance)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            }
            else if (bigCameraShakeDistance > 0 && shakeDistance < bigCameraShakeDistance)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
            else if (longCameraShakeDistance > 0 && shakeDistance < longCameraShakeDistance)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
            }
            else if (veryStrongCameraShakeDistance > 0 && shakeDistance < veryStrongCameraShakeDistance)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
            }

            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            base.CheckObjectsInRegion();

            float sqrMaxDamageRange = (regionCollider.bounds.max - regionCollider.bounds.center).sqrMagnitude;

            HashSet<IHittable> hittablesHit = [with(objectsFound)];
            int[]? enemyAmounts = null;

            for (int i = 0; i < objectsFound; i++)
            {
                Collider? colliderHit = overlapBuffer?[i];

                // Skip disabled colliders.
                if (colliderHit == null || !colliderHit.enabled)
                {
                    continue;
                }

                Transform targetTransform = colliderHit.transform;

                // Check if object hit is actually inside the region.
                if (regionCollider.ClosestPoint(targetTransform.position) != targetTransform.position)
                {
                    continue;
                }

                float sqrDistanceFromBlast = (targetTransform.position - explosionOrigin).sqrMagnitude,
                    damageTime = Mathf.Sqrt(sqrDistanceFromBlast / sqrMaxDamageRange);

                // TODO: Parameterize more stuff?
                if (!Physics.Linecast(explosionOrigin, targetTransform.position + (Vector3.up * 0.3f), coverMask,
                    QueryTriggerInteraction.Ignore))
                {
                    if (colliderHit.TryGetComponent(out PlayerControllerB player) && hittablesHit.Add(player) && player.IsOwner)
                    {
                        if (killBounds != null && killBounds.enabled && killBounds.ClosestPoint(targetTransform.position) == targetTransform.position)
                        {
                            Vector3 launchVelocity = Vector3.Normalize(player.gameplayCamera.transform.position - explosionOrigin) * 80.0f /
                                Vector3.Distance(player.gameplayCamera.transform.position, explosionOrigin);

                            player.KillPlayer(launchVelocity, true, CauseOfDeath.Blast);
                        }
                        else if (sqrDistanceFromBlast <= sqrMaxDamageRange)
                        {
                            Vector3 launchVelocity = Vector3.Normalize(player.gameplayCamera.transform.position - explosionOrigin) * 80.0f /
                                Vector3.Distance(player.gameplayCamera.transform.position, explosionOrigin);

                            player.DamagePlayer(Mathf.RoundToInt(damageCurve.Evaluate(damageTime)), true, true,
                                CauseOfDeath.Blast, 0, false, launchVelocity);

                            // TODO: Launch player.
                        }
                    }
                    else if (sqrDistanceFromBlast <= sqrMaxDamageRange)
                    {
                        if (colliderHit.TryGetComponent(out Landmine landmine) && hittablesHit.Add(landmine) && !landmine.hasExploded)
                        {
                            _ = StartCoroutine(landmine.TriggerOtherMineDelayed(landmine));
                        }
                        else if (colliderHit.TryGetComponent(out EnemyAICollisionDetect enemyCollision) && hittablesHit.Add(enemyCollision)
                            && enemyCollision.mainScript != null)
                        {
                            // Check if no enemy filters are defined.
                            if (enemyFilters == null || enemyFilters.Length == 0)
                            {
                                enemyCollision.mainScript.HitEnemyOnLocalClient(Mathf.RoundToInt(enemyDamageCurve.Evaluate(damageTime)));
                                enemyCollision.mainScript.HitFromExplosion(distance: Mathf.Sqrt(sqrDistanceFromBlast));

                                continue;
                            }

                            enemyAmounts ??= new int[enemyFilters.Length];

                            for (int j = 0; j < enemyFilters?.Length; j++)
                            {
                                EnemyFilter filter = enemyFilters[j];
                                string search = (!filter.objectSearch && enemyCollision.mainScript.enemyType != null) ? enemyCollision.mainScript.enemyType.enemyName
                                    : enemyCollision.mainScript.gameObject.name;

                                if (EnemySensor.CheckFilter(filter, search, ref enemyAmounts[j], out bool matchedBlacklist))
                                {
                                    enemyCollision.mainScript.HitEnemyOnLocalClient(Mathf.RoundToInt(enemyDamageCurve.Evaluate(damageTime)));
                                    enemyCollision.mainScript.HitFromExplosion(distance: Mathf.Sqrt(sqrDistanceFromBlast));

                                    break;
                                }

                                if (matchedBlacklist)
                                {
                                    break;
                                }
                            }
                        }
                        else if (colliderHit.TryGetComponent(out IHittable hittable) && hittablesHit.Add(hittable))
                        {
                            _ = hittable.Hit(Mathf.RoundToInt(otherDamageCurve.Evaluate(damageTime)), explosionOrigin,
                                GameNetworkManager.Instance.localPlayerController);
                        }
                    }
                }
            }

            if (parentNetworkObject != null && parentNetworkObject.IsSpawned)
            {
                // Despawn after the configured amount of time.
                _ = StartCoroutine(DespawnDelayed());
            }

            // TODO: Affect physics and vehicle.
        }

        /// <summary>
        ///     <c>Coroutine</c> to despawn after a specified amount of time passes.
        /// </summary>
        private IEnumerator DespawnDelayed()
        {
            yield return Yielders.WaitForSeconds(despawnTimer);

            // Despawn and destroy.
            if (parentNetworkObject != null)
            {
                parentNetworkObject.Despawn(true);
            }
        }
    }
}