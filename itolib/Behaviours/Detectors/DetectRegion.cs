using DunGen;
using itolib.Enums;
using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     Represents an abstract region within which to detect or perform a search for <c>Collider</c> objects with an attached <c>Behaviour</c> of type
    ///     <typeparamref name="T"/>, which are then fed to various event callbacks.
    /// </summary>
    /// <remarks><b>NOTE:</b> Region needs to be either a <c>BoxCollider</c>, <c>SphereCollider</c>, or <c>CapsuleCollider</c> to perform searches.</remarks>
    [RequireComponent(typeof(Collider))]
    public abstract class DetectRegion<T> : NetworkBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     Pre-allocated <c>Collider</c> array of a specified size (<see cref="maxObjects"/>), containing objects of type <typeparamref name="T"/>
        ///     overlapping this <c>DetectRegion</c>.
        /// </summary>
        public Collider[]? OverlapBuffer { get; private set; }

        /// <summary>
        ///     Total number of <c>Collider</c> instances found by the last search performed by this <c>DetectRegion</c>, regardless of whether or not they
        ///     are of type <typeparamref name="T"/>.
        /// </summary>
        public int ObjectsFound { get; private set; }

        /// <summary>
        ///     <c>Collider</c> whose bounds are to be used when searching for overlapping objects.
        /// </summary>
        /// <remarks><b>NOTE:</b> Region needs to be either a <c>BoxCollider</c>, <c>SphereCollider</c>, or <c>CapsuleCollider</c> to perform searches.</remarks>
        [Header("Detect Region")]
        [Tooltip("Collider whose bounds are to be used when searching for overlapping objects. NOTE: Region needs to be either a BoxCollider, SphereCollider, "
            + "or CapsuleCollider to perform searches.")]
        public Collider? regionCollider;

        /// <summary>
        ///     Activation time for the region's automatic object search.
        /// </summary>
        /// <remarks><b>NOTE:</b> Can be set to <c>Manual</c> to disable the automatic search, but is not required for triggering manual searches afterwards.</remarks>
        [Tooltip("Activation time for the region's automatic object search. NOTE: Can be set to Manual to disable the automatic search, but is not required "
            + "for triggering manual searches afterwards.")]
        public ActivationTime activationTime = ActivationTime.StartOfRound;

        /// <summary>
        ///     Maximum number of <c>Collider</c> instances expected to be found per search by this <c>DetectRegion</c>, for memory allocation purposes. Can be set
        ///     to 0 to disable object searching.
        /// </summary>
        [Tooltip("Maximum number of Collider instances expected to be found per search by this DetectRegion, for memory allocation purposes. Can be set "
            + "to 0 to disable object searching.")]
        [Min(0)]
        public int maxObjects = 16;

        /// <summary>
        ///     Callback invoked after a search is performed, with the total number of overlapping <c>Collider</c> instances found given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked after a search is performed, with the total number of overlapping Collider instances found as a parameter.")]
        [Header("Detect Region Events")]
        public UnityEvent<int>? onRegionChecked;

        /// <summary>
        ///     Callback invoked when an object of type <typeparamref name="T"/> enters the region, with the object itself given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked when an object of the defined type enters the region, with the object itself as a parameter.")]
        public UnityEvent<T>? onRegionEntered;

        /// <summary>
        ///     Callback invoked when an object of type <typeparamref name="T"/> exits the region, with the object itself given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked when an object of the defined type exits the region, with the object itself as a parameter.")]
        public UnityEvent<T>? onRegionExited;

        /// <summary>
        ///     Callback invoked sequentially on every object of type <typeparamref name="T"/> found within the region after a search is performed, with
        ///     each object given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked sequentially on every object of the defined type found within the region after a search is performed, with "
            + "each object given as a parameter.")]
        public UnityEvent<T>? onObjectsEach;

        /// <summary>
        ///     Callback invoked after a search is performed, only if at least one object of type <typeparamref name="T"/> is found within the region, with
        ///     the total number of overlapping objects given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked after a search is performed, only if at least one object of the defined type is found within the region, with "
            + "the total number of overlapping objects given as a parameter.")]
        public UnityEvent<int>? onObjectsAny;

        /// <summary>
        ///     Layers within which to search for overlapping objects of type <typeparamref name="T"/>.
        /// </summary>
        [Space(10f)]
        [Header("Layer Mask")]
        [Tooltip("Layers within which to search for overlapping objects of the defined type.")]
        public LayerMask layerMask;

        /// <summary>
        ///     Define default values for this <c>DetectRegion</c>.
        /// </summary>
        /// <remarks>Meant for defining a default <c>LayerMask</c> value (<see cref="layerMask"/>), tailored to the specific type of object to find.</remarks>
        public abstract void Reset();

        /// <summary>
        ///     Initialize buffer array and either perform a search immediately, or subscribe to a specific event depending on the set <c>ActivationTime</c>.
        /// </summary>
        public virtual void Start()
        {
            if (maxObjects > 0)
            {
                OverlapBuffer = new Collider[maxObjects];
            }

            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.AddListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.AddListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.AddListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     Perform a search every time this script is enabled, if <c>ActivationTime</c> is set to <c>Immediate</c>.
        /// </summary>
        public virtual void OnEnable()
        {
            if (activationTime is ActivationTime.Immediate)
            {
                CheckObjectsInRegion();
            }
        }

        /// <summary>
        ///     Unsubscribe to the event that may have been subscribed to, depending on the set <c>ActivationTime</c>.
        /// </summary>
        public virtual void OnDisable()
        {
            switch (activationTime)
            {
                case ActivationTime.ScrapSpawn:
                    DungeonManager.GlobalDungeonEvents?.onSpawnedScrapObjects?.RemoveListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.HazardSpawn:
                    DungeonManager.GlobalDungeonEvents?.onSpawnedMapObjects?.RemoveListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.StartOfRound:
                    StartOfRound.Instance?.StartNewRoundEvent.RemoveListener(CheckObjectsInRegion);
                    break;
                case ActivationTime.Immediate:
                case ActivationTime.DungeonComplete:
                case ActivationTime.Manual:
                default:
                    break;
            }
        }

        /// <summary>
        ///     Listener called when any <c>Collider</c> enters the region.
        /// </summary>
        /// <param name="other">Collider that entered the region.</param>
        public virtual void OnTriggerEnter(Collider other) { }

        /// <summary>
        ///     Listener called when any <c>Collider</c> exits the region.
        /// </summary>
        /// <param name="other">Collider that exited the region.</param>
        public virtual void OnTriggerExit(Collider other) { }

        /// <summary>
        ///     Perform a non-allocating search within the defined region (<see cref="regionCollider"/>), and store any found <c>Collider</c> instances into
        ///     the overlap buffer (<see cref="OverlapBuffer"/>).
        /// </summary>
        public virtual void CheckObjectsInRegion()
        {
            if (regionCollider == null || OverlapBuffer == null)
            {
                return;
            }

            ObjectsFound = 0;

            // Perform non-allocating overlapping Collider search.
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
                float offset = (capsule.height * 0.5f) - capsule.radius;

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
            // ...

            // Invoke event after the region is checked.
            onRegionChecked?.Invoke(ObjectsFound);
        }

        /// <summary>
        ///     <c>DunGen</c> listener called when generation finishes, but before blockers and connectors are placed.
        /// </summary>
        public void OnDungeonComplete(Dungeon _)
        {
            if (activationTime is ActivationTime.DungeonComplete)
            {
                CheckObjectsInRegion();
            }
        }
    }
}