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
        public bool VanillaExplosion { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Explode Effect")]
        [Tooltip("")]
        public GameObject? explosionPrefab;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool spawnExplosionEffect = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Damage")]
        [Tooltip("")]
        public AnimationCurve damageCurve = AnimationCurve.Constant(0.0f, 1.0f, 50.0f);

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AnimationCurve enemyDamageCurve = AnimationCurve.Constant(0.0f, 1.0f, 6.0f);

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public AnimationCurve otherDamageCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Collider? damageBounds;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public Collider? killBounds;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Camera Shake")]
        [Tooltip("")]
        [Min(0.0f)]
        public float smallCameraShakeDistance = 25.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        public float bigCameraShakeDistance = 14.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        public float longCameraShakeDistance = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0.0f)]
        public float veryStrongCameraShakeDistance = 0.0f;

        /// <summary>
        ///     Parent NetworkObject to despawn.
        /// </summary>
        [Header("Despawn")]
        [Tooltip("Parent NetworkObject to despawn.")]
        public NetworkObject? parentNetworkObject;

        /// <summary>
        ///     Delay in seconds until despawning, to allow effects to play.
        /// </summary>
        [Tooltip("Delay in seconds until despawning, to allow effects to play.")]
        public float despawnTimer = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Collision")]
        [Tooltip("")]
        public LayerMask coverMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Reset()
        {
            layerMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Enemies"))
                | (1 << LayerMask.NameToLayer("MapHazards"));

            coverMask = (1 << LayerMask.NameToLayer("Room")) | (1 << LayerMask.NameToLayer("Vehicle"));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Start()
        {
            base.Start();

            if (explosionPrefab == null)
            {
                explosionPrefab = StartOfRound.Instance?.explosionPrefab;
                VanillaExplosion = true;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
		public override void CheckObjectsInRegion()
        {
            if (explosionPrefab == null)
            {
                return;
            }

            Vector3 explosionOrigin = transform.position;
            Instantiate(explosionPrefab, explosionOrigin, VanillaExplosion ? Quaternion.Euler(-90f, 0f, 0f)
                : Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform).SetActive(true);

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

            base.CheckObjectsInRegion();

            if (OverlapBuffer == null || OverlapBuffer.Length == 0)
            {
                return;
            }

            bool localPlayerHit = false;

            for (int i = 0; i < ObjectsFound; i++)
            {
                Collider? colliderHit = OverlapBuffer[i];

                if (colliderHit == null || !colliderHit.enabled) // Skip disabled colliders.
                {
                    continue;
                }

                GameObject objectHit = colliderHit.gameObject;
                Transform targetTransform = objectHit.transform;

                float distanceFromBlast = Vector3.Distance(explosionOrigin, targetTransform.position);

                float? damageRange = damageBounds?.bounds.extents.sqrMagnitude,
                    killRange = killBounds?.bounds.extents.sqrMagnitude;

                float damageTime = 1 - (distanceFromBlast / damageRange) ?? 0.0f;

                // TODO: Parameterize more stuff?
                if (!Physics.Linecast(explosionOrigin, targetTransform.position + (Vector3.up * 0.3f), coverMask,
                    QueryTriggerInteraction.Ignore))
                {
                    if (!localPlayerHit && objectHit.TryGetComponent(out PlayerControllerB player) && player.IsOwner)
                    {
                        if (distanceFromBlast < killRange)
                        {
                            Vector3 launchVelocity = Vector3.Normalize(player.gameplayCamera.transform.position - explosionOrigin) * 80.0f /
                                Vector3.Distance(player.gameplayCamera.transform.position, explosionOrigin);

                            player.KillPlayer(launchVelocity, true, CauseOfDeath.Blast);
                        }
                        else if (distanceFromBlast <= damageRange)
                        {
                            Vector3 launchVelocity = Vector3.Normalize(player.gameplayCamera.transform.position - explosionOrigin) * 80.0f /
                                Vector3.Distance(player.gameplayCamera.transform.position, explosionOrigin);

                            player.DamagePlayer(Mathf.RoundToInt(damageCurve.Evaluate(damageTime)), true,
                                true, CauseOfDeath.Blast, 0, false, launchVelocity);
                        }
                    }
                    else if (objectHit.TryGetComponent(out Landmine landmine) && !landmine.hasExploded && distanceFromBlast < damageRange)
                    {
                        _ = landmine.StartCoroutine(landmine.TriggerOtherMineDelayed(landmine));
                    }
                    else if (objectHit.TryGetComponent(out EnemyAICollisionDetect enemyCollision) && enemyCollision.mainScript?.IsOwner == true
                        && distanceFromBlast < damageRange)
                    {
                        enemyCollision.mainScript.HitEnemyOnLocalClient(Mathf.RoundToInt(enemyDamageCurve.Evaluate(damageTime)));
                        enemyCollision.mainScript.HitFromExplosion(distanceFromBlast);
                    }
                    else if (objectHit.TryGetComponent(out IHittable hittable) && distanceFromBlast <= damageRange)
                    {
                        _ = hittable.Hit(Mathf.RoundToInt(otherDamageCurve.Evaluate(damageTime)), explosionOrigin);
                    }
                }
            }

            if (IsHost && parentNetworkObject != null && parentNetworkObject.IsSpawned)
            {
                // Despawn after the configured amount of time.
                _ = StartCoroutine(DespawnDelayed());
            }

            // TODO: Affect physics and vehicle.
        }

        /// <summary>
        ///     Coroutine to despawn after a specified amount of time passes.
        /// </summary>
        public virtual IEnumerator DespawnDelayed()
        {
            yield return new WaitForSeconds(despawnTimer);

            // Despawn and destroy.
            parentNetworkObject?.Despawn(true);
        }
    }
}