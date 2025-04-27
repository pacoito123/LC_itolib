using itolib.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class DetectRegion<T> : MonoBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public Collider[]? OverlapBuffer { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        public int ObjectsFound { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Detect Region")]
        [Tooltip("")]
        public Collider? regionCollider;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int maxObjects = 16;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int>? onRegionChecked;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<T>? onRegionEntered;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<T>? onRegionExited;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<T>? onObjectsEach;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<int>? onObjectsAny;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(10f)]
        [Header("Layer Mask")]
        [Tooltip("")]
        public LayerMask layerMask;

        /// <summary>
        ///     TODO.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void Start()
        {
            OverlapBuffer = new Collider[maxObjects];

            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    CheckObjectsInRegion();
                    break;
                case ActivationTime.ScrapSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.AddListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.HazardSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.AddListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.AddListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void OnEnable()
        {
            if (activationTime == ActivationTime.Immediate)
            {
                CheckObjectsInRegion();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void OnDisable()
        {
            switch (activationTime)
            {
                case ActivationTime.Immediate:
                    break;
                case ActivationTime.ScrapSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.RemoveListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.HazardSpawn:
                    LethalLevelLoader.DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.RemoveListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public virtual void OnTriggerEnter(Collider other) { }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public virtual void OnTriggerExit(Collider other) { }

        /// <summary>
        ///     TODO.
        /// </summary>
        public virtual void CheckObjectsInRegion()
        {
            if (regionCollider == null || OverlapBuffer == null)
            {
                return;
            }

            ObjectsFound = 0;

            if (regionCollider is BoxCollider box)
            {
                ObjectsFound = Physics.OverlapBoxNonAlloc(transform.TransformPoint(box.center), box.size * 0.5f, OverlapBuffer,
                    Quaternion.identity, layerMask, QueryTriggerInteraction.Collide);
            }
            else if (regionCollider is SphereCollider sphere)
            {
                ObjectsFound = Physics.OverlapSphereNonAlloc(transform.TransformPoint(sphere.center), sphere.radius, OverlapBuffer,
                    layerMask, QueryTriggerInteraction.Collide);
            }
            else if (regionCollider is CapsuleCollider capsule)
            {
                Vector3 direction = new() { [capsule.direction] = 1 };
                float offset = (capsule.height / 2) - capsule.radius;

                // TODO: Test if works when rotated.
                ObjectsFound = Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(capsule.center - (offset * direction)),
                    transform.TransformPoint(capsule.center + (offset * direction)), capsule.radius, OverlapBuffer, layerMask,
                    QueryTriggerInteraction.Collide);
            }
            else
            {
                Plugin.StaticLogger.LogWarning($"Unsupported Collider shape used for DetectRegion '{gameObject.name}'! It must be either a "
                    + "BoxCollider, SphereCollider, or CapsuleCollider to perform the query.");
                return;
            }

            onRegionChecked?.Invoke(ObjectsFound);
        }
    }
}