using GameNetcodeStuff;
using itolib.Behaviours.Detectors;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ExplodeEffect : DetectRegion<GameObject>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Explode Effect")]
        [Tooltip("")]
        [SerializeField] private GameObject? explosionPrefab;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool spawnExplosionEffect = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Damage")]
        [Tooltip("")]
        [SerializeField] private AnimationCurve damageCurve = AnimationCurve.Constant(0.0f, 1.0f, 50.0f);

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AnimationCurve enemyDamageCurve = AnimationCurve.Constant(0.0f, 1.0f, 6.0f);

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private AnimationCurve otherDamageCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private Collider? killBounds;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Camera Shake")]
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] private float smallCameraShakeDistance = 25.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] private float bigCameraShakeDistance = 14.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] private float longCameraShakeDistance = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        [SerializeField] private float veryStrongCameraShakeDistance = 0.0f;

        /// <summary>
        ///     Parent NetworkObject to despawn.
        /// </summary>
        [Header("Despawn")]
        [Tooltip("Parent NetworkObject to despawn.")]
        [SerializeField] private NetworkObject? parentNetworkObject;

        /// <summary>
        ///     Delay in seconds until despawning, to allow effects to play.
        /// </summary>
        [Tooltip("Delay in seconds until despawning, to allow effects to play.")]
        [SerializeField] private float despawnTimer = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Collision")]
        [Tooltip("")]
        [SerializeField] private LayerMask coverMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool useVanillaExplosion;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Reset()
        {
            layerMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Enemies"))
                | (1 << LayerMask.NameToLayer("MapHazards"));

            coverMask = (1 << LayerMask.NameToLayer("Room")) | (1 << LayerMask.NameToLayer("Vehicle"));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (explosionPrefab == null)
            {
                explosionPrefab = StartOfRound.Instance != null ? StartOfRound.Instance.explosionPrefab : null;
                useVanillaExplosion = true;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
		public override void CheckObjectsInRegion()
        {
            if (explosionPrefab == null || regionCollider == null)
            {
                return;
            }

            float sqrMaxDamageRange = (regionCollider.bounds.max - regionCollider.bounds.center).sqrMagnitude;

            Vector3 explosionOrigin = (regionCollider != null && regionCollider.enabled && regionCollider.gameObject.activeInHierarchy)
                ? regionCollider.bounds.center : transform.position;
            if (spawnExplosionEffect)
            {
                Instantiate(explosionPrefab, explosionOrigin, useVanillaExplosion ? Quaternion.Euler(-90f, 0f, 0f)
                    : Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform).SetActive(true);
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

            if (!IsHost)
            {
                return;
            }

            base.CheckObjectsInRegion();

            if (overlapBuffer == null || overlapBuffer.Length == 0)
            {
                return;
            }

            bool localPlayerHit = false;

            for (int i = 0; i < objectsFound; i++)
            {
                Collider? colliderHit = overlapBuffer[i];

                if (colliderHit == null || !colliderHit.enabled) // Skip disabled colliders.
                {
                    continue;
                }

                GameObject objectHit = colliderHit.gameObject;
                Transform targetTransform = objectHit.transform;

                float sqrDistanceFromBlast = (targetTransform.position - explosionOrigin).sqrMagnitude,
                    damageTime = sqrDistanceFromBlast / sqrMaxDamageRange;

                // TODO: Parameterize more stuff?
                if (!Physics.Linecast(explosionOrigin, targetTransform.position + (Vector3.up * 0.3f), coverMask,
                    QueryTriggerInteraction.Ignore))
                {
                    if (!localPlayerHit && objectHit.TryGetComponent(out PlayerControllerB player) && player.IsOwner)
                    {
                        if (killBounds != null && killBounds.bounds.Contains(targetTransform.position))
                        {
                            Vector3 launchVelocity = Vector3.Normalize(player.gameplayCamera.transform.position - explosionOrigin) * 80.0f /
                                Vector3.Distance(player.gameplayCamera.transform.position, explosionOrigin);

                            player.KillPlayer(launchVelocity, true, CauseOfDeath.Blast);
                        }
                        else if (sqrDistanceFromBlast <= sqrMaxDamageRange)
                        {
                            Vector3 launchVelocity = Vector3.Normalize(player.gameplayCamera.transform.position - explosionOrigin) * 80.0f /
                                Vector3.Distance(player.gameplayCamera.transform.position, explosionOrigin);

                            player.DamagePlayer(Mathf.RoundToInt(damageCurve.Evaluate(damageTime)), true,
                                true, CauseOfDeath.Blast, 0, false, launchVelocity);
                        }
                    }
                    else if (sqrDistanceFromBlast <= sqrMaxDamageRange)
                    {
                        if (objectHit.TryGetComponent(out Landmine landmine) && !landmine.hasExploded)
                        {
                            _ = landmine.StartCoroutine(landmine.TriggerOtherMineDelayed(landmine));
                        }
                        else if (objectHit.TryGetComponent(out EnemyAICollisionDetect enemyCollision) && enemyCollision.mainScript != null
                            && enemyCollision.mainScript.IsOwner)
                        {
                            enemyCollision.mainScript.HitEnemyOnLocalClient(Mathf.RoundToInt(enemyDamageCurve.Evaluate(damageTime)));
                            enemyCollision.mainScript.HitFromExplosion(distance: 0.0f); // Distance parameter appears to be unused.
                        }
                        else if (objectHit.TryGetComponent(out IHittable hittable))
                        {
                            _ = hittable.Hit(Mathf.RoundToInt(otherDamageCurve.Evaluate(damageTime)), explosionOrigin);
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
        ///     Coroutine to despawn after a specified amount of time passes.
        /// </summary>
        private IEnumerator DespawnDelayed()
        {
            yield return new WaitForSeconds(despawnTimer);

            // Despawn and destroy.
            if (parentNetworkObject != null)
            {
                parentNetworkObject.Despawn(true);
            }
        }
    }
}