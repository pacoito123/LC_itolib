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
    [RequireComponent(typeof(ItemGrabbable))]
    public class ItemWhackable : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public PlayerControllerB? LastHeldBy { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public RaycastHit[]? HitBuffer { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public List<EnemyAI>? HitEnemies { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public int ObjectsHit { get; private set; }

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
        [HideInInspector]
        public bool isHoldingButton;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public bool reelingUp;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public Coroutine? whackingCoroutine;

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
        public void Awake()
        {
            if (item == null && !TryGetComponent(out item))
            {
                // TODO: Log warning
                enabled = false;

                return;
            }

            item.onActivate.AddListener(ItemActivate);
            item.onDiscardEarly.AddListener(DiscardItemEarly);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Start()
        {
            HitBuffer = new RaycastHit[maxObjectHits];
            HitEnemies = new(maxObjectHits);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="used"></param>
        /// <param name="buttonDown"></param>
        public void ItemActivate(bool used, bool buttonDown)
        {
            if (item.playerHeldBy == null)
            {
                return;
            }

            isHoldingButton = buttonDown;

            if (!reelingUp && isHoldingButton)
            {
                reelingUp = true;
                LastHeldBy = item.playerHeldBy;

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
            if (LastHeldBy == null)
            {
                yield break;
            }

            LastHeldBy.activatingItem = true;
            LastHeldBy.twoHanded = true;
            LastHeldBy.playerBodyAnimator.ResetTrigger("shovelHit");
            LastHeldBy.playerBodyAnimator.SetBool("reelingUp", true);

            onReelingStart.Invoke();
            yield return new WaitForSeconds(chargeTimer);

            onReelingFinish.Invoke();
            yield return new WaitUntil(() => !isHoldingButton || !item.isHeld);

            // Handle swing.
            LastHeldBy.playerBodyAnimator.SetBool("reelingUp", false);
            if (item.isHeld)
            {
                onWeaponSwing.Invoke();
                LastHeldBy.UpdateSpecialAnimationValue(true, (short)LastHeldBy.transform.localEulerAngles.y, 0.4f, false);
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
        public void DiscardItemEarly()
        {
            if (LastHeldBy != null)
            {
                LastHeldBy.activatingItem = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Whack()
        {
            if (LastHeldBy == null)
            {
                return;
            }

            LastHeldBy.activatingItem = false;

            if (!item.isHeld)
            {
                return;
            }

            LastHeldBy.twoHanded = false;

            bool weaponHit = false, enemyHit = false, playerHit = false;
            int surfaceIndex = -1;

            Transform gameplayCamera = LastHeldBy.gameplayCamera.transform;

            // TODO: Parameterize hit position, radius, and distance.
            ObjectsHit = Physics.SphereCastNonAlloc(gameplayCamera.position + (-0.35f * gameplayCamera.right), 0.8f,
                gameplayCamera.forward, HitBuffer, 1.5f, shovelMask, QueryTriggerInteraction.Collide);

            if (HitBuffer == null || ObjectsHit == 0)
            {
                return;
            }

            for (int i = 0; i < ObjectsHit; i++)
            {
                RaycastHit rayHit = HitBuffer[i];
                GameObject objectHit = rayHit.transform.gameObject;

                if (((1 << objectHit.layer) & hitSFXMask) != 0)
                {
                    if (!rayHit.collider.isTrigger)
                    {
                        weaponHit = true;

                        for (int j = 0; j < StartOfRound.Instance.footstepSurfaces.Length; j++)
                        {
                            if (string.CompareOrdinal(StartOfRound.Instance.footstepSurfaces[j].surfaceTag, objectHit.tag) == 0)
                            {
                                surfaceIndex = j;

                                break;
                            }
                        }
                    }
                }
                else if (objectHit.TryGetComponent(out IHittable hittable) && rayHit.transform != LastHeldBy.transform && (rayHit.point == Vector3.zero
                    || !Physics.Linecast(gameplayCamera.position, rayHit.point, hitMask, QueryTriggerInteraction.Ignore)))
                {
                    weaponHit = true;

                    if (objectHit.TryGetComponent(out EnemyAICollisionDetect enemyAICollision) &&
                        (enemyAICollision.mainScript == null || HitEnemies?.Contains(enemyAICollision.mainScript) == true))
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

                    if (hittable.Hit(weaponDamage, gameplayCamera.forward, LastHeldBy, playHitSFX, hitID)
                        && enemyAICollision != null)
                    {
                        HitEnemies?.Add(enemyAICollision.mainScript);
                        enemyHit = true;
                    }
                }
            }

            HitEnemies?.Clear();

            if (weaponHit)
            {
                WeaponHitLocal(enemyHit, surfaceIndex);
                WeaponHitServerRpc(LastHeldBy, enemyHit, surfaceIndex);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="playerReference"></param>
        /// <param name="enemyHit"></param>
        /// <param name="surfaceIndex"></param>
        [ServerRpc(RequireOwnership = false)]
        public void WeaponHitServerRpc(NetworkBehaviourReference playerReference, bool enemyHit, int surfaceIndex)
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
        public void WeaponHitClientRpc(NetworkBehaviourReference playerReference, bool enemyHit, int surfaceIndex)
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

            if (LastHeldBy != null)
            {
                LastHeldBy.playerBodyAnimator.SetTrigger("shovelHit");
            }
        }
    }
}