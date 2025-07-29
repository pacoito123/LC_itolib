using GameNetcodeStuff;
using itolib.Extensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Grabbables
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class ItemWhackable : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Whackable")]
        [Tooltip("")]
        public ItemGrabbable item = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int weaponDamage = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int maxObjectHits = 16;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int hitID = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Audio")]
        [Tooltip("")]
        public AudioSource? weaponAudio;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool playHitSFX = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool silentHit = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Speed")]
        [Tooltip("")]
        public float chargeTimer = 0.35f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float hitSpeed = 0.13f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public float hitCooldown = 0.3f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent onReelingStart = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onReelingFinish = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onWeaponSwing = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onWeaponHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent onWeaponHitLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onWeaponHitVariant = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onWeaponHitVariantLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onSurfaceHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int> onSurfaceHitLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(10f)]
        [Header("Collision")]
        [Tooltip("")]
        public LayerMask shovelMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public LayerMask hitMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public LayerMask hitSFXMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        private PlayerControllerB? lastHeldBy;

        /// <summary>
        ///     TODO.
        /// </summary>
        private RaycastHit[]? hitBuffer;

        /// <summary>
        ///     TODO.
        /// </summary>
        private List<EnemyAI>? hitEnemies;

        /// <summary>
        ///     TODO.
        /// </summary>
        private int objectsHit;

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool isHoldingButton;

        /// <summary>
        ///     TODO.
        /// </summary>
        private bool reelingUp;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Coroutine? whackingCoroutine;

        private void Reset()
        {
            shovelMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Props"))
                | (1 << LayerMask.NameToLayer("Room")) | (1 << LayerMask.NameToLayer("Colliders"))
                | (1 << LayerMask.NameToLayer("Enemies")) | (1 << LayerMask.NameToLayer("MapHazards"))
                | (1 << LayerMask.NameToLayer("EnemiesNotRendered")) | (1 << LayerMask.NameToLayer("Vehicle"));

            hitMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Room"))
                | (1 << LayerMask.NameToLayer("Colliders")) | (1 << LayerMask.NameToLayer("Terrain"))
                | (1 << LayerMask.NameToLayer("Vehicle"));

            hitSFXMask = (1 << LayerMask.NameToLayer("Room")) | (1 << LayerMask.NameToLayer("Colliders"));
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (item == null && !TryGetComponent(out item))
            {
                // TODO: Log warning
                enabled = false;

                return;
            }

            item.OnActivate.AddListener(ItemActivate);
            item.OnDiscardEarly.AddListener(DiscardItemEarly);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Start()
        {
            hitBuffer = new RaycastHit[maxObjectHits];
            hitEnemies = new(maxObjectHits);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="used"></param>
        /// <param name="buttonDown"></param>
        private void ItemActivate(bool used, bool buttonDown)
        {
            if (item.playerHeldBy == null)
            {
                return;
            }

            isHoldingButton = buttonDown;

            if (!reelingUp && isHoldingButton)
            {
                reelingUp = true;
                lastHeldBy = item.playerHeldBy;

                if (whackingCoroutine != null)
                {
                    StopCoroutine(whackingCoroutine);
                }

                whackingCoroutine = StartCoroutine(HandleWhacking());
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        private IEnumerator HandleWhacking()
        {
            if (lastHeldBy == null)
            {
                yield break;
            }

            lastHeldBy.activatingItem = true;
            lastHeldBy.twoHanded = true;
            lastHeldBy.playerBodyAnimator.ResetTrigger("shovelHit"); // TODO: Use ID
            lastHeldBy.playerBodyAnimator.SetBool("reelingUp", true);

            onReelingStart.Invoke();
            yield return new WaitForSeconds(chargeTimer);

            onReelingFinish.Invoke();
            yield return new WaitUntil(() => !isHoldingButton || !item.isHeld);

            // Handle swing.
            lastHeldBy.playerBodyAnimator.SetBool("reelingUp", false);
            if (item.isHeld)
            {
                onWeaponSwing.Invoke();
                lastHeldBy.UpdateSpecialAnimationValue(true, (short)lastHeldBy.transform.localEulerAngles.y, 0.4f, false);
            }
            // ...

            yield return new WaitForSeconds(hitSpeed);
            yield return new WaitForEndOfFrame();

            Whack(); // Bonk.mp3

            yield return new WaitForSeconds(hitCooldown);

            reelingUp = false;
            whackingCoroutine = null;

            yield break;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void DiscardItemEarly()
        {
            if (lastHeldBy != null)
            {
                lastHeldBy.activatingItem = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Whack()
        {
            if (lastHeldBy == null)
            {
                return;
            }

            lastHeldBy.activatingItem = false;

            if (!item.isHeld)
            {
                return;
            }

            lastHeldBy.twoHanded = false;

            bool weaponHit = false, enemyHit = false, playerHit = false;
            int surfaceIndex = -1;

            Transform gameplayCamera = lastHeldBy.gameplayCamera.transform;

            // TODO: Parameterize hit position, radius, and distance.
            objectsHit = Physics.SphereCastNonAlloc(gameplayCamera.position + (-0.35f * gameplayCamera.right), 0.8f,
                gameplayCamera.forward, hitBuffer, 1.5f, shovelMask, QueryTriggerInteraction.Collide);

            if (hitBuffer == null || objectsHit == 0)
            {
                return;
            }

            for (int i = 0; i < objectsHit; i++)
            {
                RaycastHit rayHit = hitBuffer[i];
                GameObject objectHit = rayHit.transform.gameObject;

                if (((1 << objectHit.layer) & hitSFXMask) != 0)
                {
                    if (!rayHit.collider.isTrigger)
                    {
                        weaponHit = true;

                        for (int j = 0; j < StartOfRound.Instance.footstepSurfaces.Length; j++)
                        {
                            if (objectHit.tag.CompareOrdinal(StartOfRound.Instance.footstepSurfaces[j].surfaceTag))
                            {
                                surfaceIndex = j;

                                break;
                            }
                        }
                    }
                }
                else if (objectHit.TryGetComponent(out IHittable hittable) && rayHit.transform != lastHeldBy.transform && (rayHit.point == Vector3.zero
                    || !Physics.Linecast(gameplayCamera.position, rayHit.point, hitMask, QueryTriggerInteraction.Ignore)))
                {
                    weaponHit = true;

                    if (objectHit.TryGetComponent(out EnemyAICollisionDetect enemyAICollision) &&
                        (enemyAICollision.mainScript == null || hitEnemies?.Contains(enemyAICollision.mainScript) == true))
                    {
                        continue;
                    }

                    if (objectHit.TryGetComponent(out PlayerControllerB _))
                    {
                        if (playerHit)
                        {
                            continue;
                        }

                        playerHit = true;
                    }

                    if (hittable.Hit(weaponDamage, gameplayCamera.forward, lastHeldBy, playHitSFX, hitID)
                        && enemyAICollision != null)
                    {
                        hitEnemies?.Add(enemyAICollision.mainScript);
                        enemyHit = true;
                    }
                }
            }

            hitEnemies?.Clear();

            if (weaponHit)
            {
                WeaponHitLocal(enemyHit, surfaceIndex);
                WeaponHitServerRpc(lastHeldBy, enemyHit, surfaceIndex);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="enemyHit"></param>
        /// <param name="surfaceIndex"></param>
        [ServerRpc(RequireOwnership = false)]
        private void WeaponHitServerRpc(NetworkBehaviourReference playerReference, bool enemyHit, int surfaceIndex)
        {
            WeaponHitClientRpc(playerReference, enemyHit, surfaceIndex);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="enemyHit"></param>
        /// <param name="surfaceIndex"></param>
        [ClientRpc]
        private void WeaponHitClientRpc(NetworkBehaviourReference playerReference, bool enemyHit, int surfaceIndex)
        {
            if (playerReference.TryGet(out PlayerControllerB player) && !player.IsLocalClient())
            {
                WeaponHitLocal(enemyHit, surfaceIndex);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyHit"></param>
        /// <param name="surfaceIndex"></param>
        private void WeaponHitLocal(bool enemyHit, int surfaceIndex)
        {
            if (item.VariantIndex < 0)
            {
                if (item.IsOwner)
                {
                    onWeaponHitLocal.Invoke();
                }

                onWeaponHit.Invoke();
            }
            else
            {
                if (item.IsOwner)
                {
                    onWeaponHitVariantLocal.Invoke(item.VariantIndex);
                }

                onWeaponHitVariant.Invoke(item.VariantIndex);
            }

            if (!enemyHit && surfaceIndex != -1)
            {
                if (item.IsOwner)
                {
                    onSurfaceHitLocal.Invoke(surfaceIndex);
                }

                onSurfaceHit.Invoke(surfaceIndex);
            }

            if (!silentHit)
            {
                // TODO: Parameterize noise properties
                RoundManager.Instance.PlayAudibleNoise(transform.position, 17f, 0.8f, 0, false, 0);
            }

            if (lastHeldBy != null)
            {
                lastHeldBy.playerBodyAnimator.SetTrigger("shovelHit");
            }
        }
    }
}