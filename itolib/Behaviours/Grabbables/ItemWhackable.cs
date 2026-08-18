using GameNetcodeStuff;
using itolib.Enums;
using itolib.Extensions;
using itolib.Interfaces;
using itolib.Util;
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
        ///     Hash of the trigger parameter to begin a <c>Shovel</c> hit.
        /// </summary>
        private static readonly int shovelHitID = Animator.StringToHash("shovelHit");

        /// <summary>
        ///     Hash of the bool parameter to reel up a <c>Shovel</c>.
        /// </summary>
        private static readonly int reelingUpID = Animator.StringToHash("reelingUp");

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Item Whackable")]
        [Tooltip("")]
        [SerializeField] private GrabbableObject? item;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private int weaponDamage = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private int maxObjectHits = 16;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private WeaponHitID hitID = WeaponHitID.Shovel;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Audio")]
        [Tooltip("")]
        [SerializeField] private bool playHitSFX = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool silentHit = false;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Speed")]
        [Tooltip("")]
        [SerializeField] private float chargeTimer = 0.35f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private float hitSpeed = 0.13f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private float hitCooldown = 0.3f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent onReelingStart = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onReelingFinish = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onWeaponSwing = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onWeaponHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent onWeaponHitLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onWeaponHitVariant = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onWeaponHitVariantLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onSurfaceHit = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<int> onSurfaceHitLocal = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(10f)]
        [Header("Collision")]
        [Tooltip("")]
        [SerializeField] private LayerMask shovelMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private LayerMask hitMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private LayerMask hitSFXMask;

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

        /// <summary>
        ///     TODO.
        /// </summary>
        private WaitUntil? waitUntilReelingStop;

        /// <summary>
        ///     TODO.
        /// </summary>
        private IEventfulItem? eventfulSelf;

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Reset()
        {
            shovelMask = LayerMask.GetMask("Player", "Props", "Room", "Colliders", "Enemies", "MapHazards", "EnemiesNotRendered", "Vehicle");
            hitMask = LayerMask.GetMask("Default", "Room", "Colliders", "Terrain", "Vehicle");
            hitSFXMask = LayerMask.GetMask("Room", "Colliders");
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Awake()
        {
            if (item == null || !TryGetComponent(out item) || item is not IEventfulItem eventfulItem)
            {
                Plugin.Logger.LogWarning($"Could not find IEventfulItem for Itemwhackable component in GameObject '{gameObject.name}'.");
                enabled = false;

                return;
            }

            eventfulItem.OnActivate.AddListener(ItemActivate);
            eventfulItem.OnDiscardEarly.AddListener(DiscardItemEarly);

            eventfulSelf = eventfulItem;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Start()
        {
            hitBuffer = new RaycastHit[maxObjectHits];
            hitEnemies = new(maxObjectHits);

            waitUntilReelingStop = new WaitUntil(() => !isHoldingButton || item == null || !item.isHeld);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="used"></param>
        /// <param name="buttonDown"></param>
        private void ItemActivate(bool used, bool buttonDown)
        {
            if (item == null || item.playerHeldBy == null)
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
            if (lastHeldBy == null || lastHeldBy.playerBodyAnimator == null)
            {
                yield break;
            }

            lastHeldBy.activatingItem = true;
            lastHeldBy.twoHanded = true;
            lastHeldBy.playerBodyAnimator.ResetTrigger(shovelHitID);
            lastHeldBy.playerBodyAnimator.SetBool(reelingUpID, true);

            onReelingStart.Invoke();
            yield return Yielders.WaitForSeconds(chargeTimer);

            onReelingFinish.Invoke();
            yield return waitUntilReelingStop;

            // Handle swing.
            lastHeldBy.playerBodyAnimator.SetBool(reelingUpID, false);
            if (item != null && item.isHeld)
            {
                onWeaponSwing.Invoke();
                lastHeldBy.UpdateSpecialAnimationValue(true, (short)lastHeldBy.transform.localEulerAngles.y, 0.4f, false);
            }
            // ...

            yield return Yielders.WaitForSeconds(hitSpeed);
            yield return Yielders.WaitForEndOfFrame;

            Whack(); // Bonk.mp3

            yield return Yielders.WaitForSeconds(hitCooldown);

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

            if (item == null || !item.isHeld)
            {
                return;
            }

            if (!item.itemProperties.twoHanded)
            {
                lastHeldBy.twoHanded = false;
            }

            bool weaponHit = false, enemyHit = false, playerHit = false;
            int surfaceIndex = -1;

            Transform? gameplayCamera = lastHeldBy.gameplayCamera != null ? lastHeldBy.gameplayCamera.transform : null;

            if (gameplayCamera == null)
            {
                return;
            }

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

                        // TODO: Soft compat with custom footstep 'tags'.
                        for (int j = 0; j < StartOfRound.Instance.footstepSurfaces.Length; j++)
                        {
                            if (objectHit.CompareTag(StartOfRound.Instance.footstepSurfaces[j].surfaceTag))
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

                    if (hittable.Hit(weaponDamage, gameplayCamera.forward, lastHeldBy, playHitSFX, (int)hitID)
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

                if (IsSpawned)
                {
                    WeaponHitRpc(enemyHit, surfaceIndex);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyHit"></param>
        /// <param name="surfaceIndex"></param>
        [Rpc(SendTo.NotMe, RequireOwnership = false)]
        private void WeaponHitRpc(bool enemyHit, int surfaceIndex)
        {
            WeaponHitLocal(enemyHit, surfaceIndex);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemyHit"></param>
        /// <param name="surfaceIndex"></param>
        private void WeaponHitLocal(bool enemyHit, int surfaceIndex)
        {
            if (eventfulSelf == null)
            {
                if (item is not IEventfulItem eventfulItem)
                {
                    return;
                }

                eventfulSelf = eventfulItem;
            }

            if (eventfulSelf.VariantIndex < 0)
            {
                if (item != null && item.playerHeldBy != null && item.playerHeldBy.IsLocalClient())
                {
                    onWeaponHitLocal.Invoke();
                }

                onWeaponHit.Invoke();
            }
            else
            {
                if (item != null && item.playerHeldBy != null && item.playerHeldBy.IsLocalClient())
                {
                    onWeaponHitVariantLocal.Invoke(eventfulSelf.VariantIndex);
                }

                onWeaponHitVariant.Invoke(eventfulSelf.VariantIndex);
            }

            if (!enemyHit && surfaceIndex != -1)
            {
                if (item != null && item.playerHeldBy != null && item.playerHeldBy.IsLocalClient())
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
                lastHeldBy.playerBodyAnimator.SetTrigger(shovelHitID);
            }
        }
    }
}