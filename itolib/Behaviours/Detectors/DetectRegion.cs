using DunGen;
using itolib.Enums;
using itolib.Interfaces;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     Represents an abstract region within which to detect or perform a search for <c>Collider</c> objects with an attached <c>Behaviour</c> of type
    ///     <c><typeparamref name="T"/></c>, which are then fed to various event callbacks.
    /// </summary>
    /// <remarks><b>NOTE:</b> Region needs to be either a <c>BoxCollider</c>, <c>SphereCollider</c>, or <c>CapsuleCollider</c> to perform searches.</remarks>
    [RequireComponent(typeof(Collider))]
    public abstract class DetectRegion<T> : NetworkBehaviour, IActivationScript where T : Behaviour
    {
        /// <summary>
        ///     Cached instance of the current <c>DetectRegion</c> as an <c>IActivationScript</c>, to avoid having to cast.
        /// </summary>
        public IActivationScript ActivationSelf { get; }

        /// <summary>
        ///     Whether activation has already been performed or not.
        /// </summary>
        public bool PerformedActivation { get; set; }

        /// <summary>
        ///     <c>Collider</c> whose bounds are to be used when searching for overlapping objects.
        /// </summary>
        /// <remarks><b>NOTE:</b> Region needs to be either a <c>BoxCollider</c>, <c>SphereCollider</c>, or <c>CapsuleCollider</c> to perform searches.</remarks>
        [Header("Detect Region")]
        [Tooltip("Collider whose bounds are to be used when searching for overlapping objects. NOTE: Region needs to be either a BoxCollider, SphereCollider, "
            + "or CapsuleCollider to perform searches.")]
        [SerializeField] protected Collider? regionCollider;

        /// <summary>
        ///     Desired <c>ActivationTime</c> for the region's automatic object search.
        /// </summary>
        /// <remarks><b>NOTE:</b> Can be set to <c>Manual</c> to disable the automatic search, but is not required for triggering manual searches afterwards.</remarks>
        [field: Tooltip("Desired activation time for the region's automatic object search. NOTE: Can be set to 'Manual' to disable the automatic search, "
            + "but is not required for triggering manual searches afterwards.")]
        [field: SerializeField] public ActivationTime ActivationTime { get; set; } = ActivationTime.StartOfRound;

        /// <summary>
        ///     Maximum number of <c>Collider</c> instances expected to be found per search by this <c>DetectRegion</c>, for memory allocation purposes. Can be set
        ///     to 0 to disable object searching.
        /// </summary>
        [Tooltip("Maximum number of Collider instances expected to be found per search by this DetectRegion, for memory allocation purposes. Can be set "
            + "to 0 to disable object searching.")]
        [Min(0)]
        [SerializeField] protected int maxObjects = 16;

        /// <summary>
        ///     Callback invoked after a search is performed, with the total number of overlapping <c>Collider</c> instances found given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked after a search is performed, with the total number of overlapping Collider instances found as a parameter.")]
        [Header("Detect Region Events")]
        [SerializeField] protected UnityEvent<int> onRegionChecked = new();

        /// <summary>
        ///     Callback invoked when an object of type <c><typeparamref name="T"/></c> enters the region, with the object itself given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked when an object of the defined type enters the region, with the object itself as a parameter.")]
        [SerializeField] protected UnityEvent<T> onRegionEntered = new();

        /// <summary>
        ///     Callback invoked when an object of type <c><typeparamref name="T"/></c> exits the region, with the object itself given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked when an object of the defined type exits the region, with the object itself as a parameter.")]
        [SerializeField] protected UnityEvent<T> onRegionExited = new();

        /// <summary>
        ///     Callback invoked sequentially on every object of type <c><typeparamref name="T"/></c> found within the region after a search is performed, with
        ///     each object given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked sequentially on every object of the defined type found within the region after a search is performed, with "
            + "each object given as a parameter.")]
        [SerializeField] protected UnityEvent<T> onObjectsEach = new();

        /// <summary>
        ///     Callback invoked after a search is performed, only if at least one object of type <c><typeparamref name="T"/></c> is found within the region, with
        ///     the total number of overlapping objects given as a parameter.
        /// </summary>
        [Tooltip("Callback invoked after a search is performed, only if at least one object of the defined type is found within the region, with "
            + "the total number of overlapping objects given as a parameter.")]
        [SerializeField] protected UnityEvent<int> onObjectsAny = new();

        /// <summary>
        ///     Layers within which to search for overlapping objects of type <c><typeparamref name="T"/></c>.
        /// </summary>
        [Space(10.0f)]
        [Header("Layer Mask")]
        [Tooltip("Layers within which to search for overlapping objects of the defined type.")]
        [SerializeField] protected LayerMask layerMask;

        /// <summary>
        ///     Set containing objects of type <c><typeparamref name="T"/></c> that have entered the region.
        /// </summary>
        private HashSet<T>? objectsInside;

        /// <summary>
        ///     Set containing objects of type <c><typeparamref name="T"/></c> that are to be removed from the region.
        /// </summary>
        private HashSet<T>? objectsRemoved;

        /// <summary>
        ///     Pre-allocated <c>Collider</c> array of a specified size (<see cref="maxObjects"/>), containing objects of type <c><typeparamref name="T"/></c>
        ///     overlapping this <c>DetectRegion</c>.
        /// </summary>
        protected Collider[]? overlapBuffer;

        /// <summary>
        ///     Total number of <c>Collider</c> instances found by the last search performed by this <c>DetectRegion</c>, regardless of whether or not they
        ///     are of type <c><typeparamref name="T"/></c>.
        /// </summary>
        protected int objectsFound;

        /// <summary>
        ///     Define default values for this <c>DetectRegion</c>.
        /// </summary>
        /// <remarks>Meant for defining a default <c>LayerMask</c> value (<see cref="layerMask"/>), tailored to the specific type of object to find.</remarks>
        protected virtual void Reset()
        {
            regionCollider = GetComponent<Collider>();
        }

        /// <summary>
        ///     Cache already-cast <c>IActivationScript</c> instance.
        /// </summary>
        protected DetectRegion()
        {
            ActivationSelf = this;
        }

        /// <summary>
        ///     Handle additional initialization.
        /// </summary>
        protected abstract void Awake();

        /// <summary>
        ///     Initialize collections used by this <c>DetectRegion</c>.
        /// </summary>
        protected virtual void Start()
        {
            if (maxObjects > 0)
            {
                // Initialize buffer array with the specified capacity.
                overlapBuffer = new Collider[maxObjects];
            }

            objectsInside = new(maxObjects);
            objectsRemoved = new(maxObjects);
        }

        /// <summary>
        ///      Perform search or subscribe to a specific event, depending on the set <c>ActivationTime</c>.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsHost)
            {
                return;
            }

            ActivationSelf.Initialize();
        }

        /// <summary>
        ///     Handle keeping track of objects that have entered the region, to (more) reliably call exit events.
        /// </summary>
        protected virtual void Update()
        {
            if (regionCollider == null || !regionCollider.enabled)
            {
                if (objectsInside?.Count > 0)
                {
                    foreach (T objectInside in objectsInside)
                    {
                        OnRegionEnter(objectInside, exiting: true);
                    }

                    objectsInside.Clear();
                }

                objectsRemoved?.Clear();
                enabled = false;
                return;
            }

            if (objectsInside?.Count > 0)
            {
                foreach (T objectInside in objectsInside)
                {
                    if (objectInside == null || !objectInside.enabled || !objectInside.isActiveAndEnabled)
                    {
                        OnRegionEnter(objectInside, exiting: true);
                    }
                }
            }

            if (objectsRemoved?.Count > 0)
            {
                objectsInside?.ExceptWith(objectsRemoved);
                objectsRemoved.Clear();
            }
        }

        /// <summary>
        ///     Call exit events upon disabling the current region with objects inside.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (objectsInside?.Count > 0)
            {
                foreach (T objectInside in objectsInside)
                {
                    OnRegionEnter(objectInside, exiting: true);
                }

                objectsInside.Clear();
            }
        }

        /// <summary>
        ///     Unsubscribe from events for automatic activation.
        /// </summary>
        public override void OnDestroy()
        {
            ActivationSelf.UnsubscribeFromEvents();

            base.OnDestroy();
        }

        /// <summary>
        ///     Listener called when any <c>Collider</c> enters the region.
        /// </summary>
        /// <param name="other">Collider that entered the region.</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out T enteringBehaviour))
            {
                OnRegionEnter(enteringBehaviour);
            }
        }

        /// <summary>
        ///     Listener called when any <c>Collider</c> exits the region.
        /// </summary>
        /// <param name="other">Collider that exited the region.</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out T exitingBehaviour))
            {
                OnRegionEnter(exitingBehaviour, exiting: true);
            }
        }

        /// <summary>
        ///     Handle objects of type <c><typeparamref name="T"/></c> currently entering and/or exiting the region.
        /// </summary>
        /// <param name="behaviour"><c>Behaviour</c> of type <c><typeparamref name="T"/></c> entering or exiting the region.</param>
        /// <param name="exiting">Whether the region is being entered or exited.</param>
        protected virtual void OnRegionEnter(T? behaviour, bool exiting = false)
        {
            if (behaviour == null)
            {
                return;
            }

            if (!exiting && objectsInside?.Add(behaviour) == true)
            {
                onRegionEntered.Invoke(behaviour);
            }
            else if (exiting && objectsRemoved?.Add(behaviour) == true)
            {
                onRegionExited.Invoke(behaviour);
            }
        }

        /// <summary>
        ///     Perform a non-allocating search within the defined region (<see cref="regionCollider"/>), and store any found <c>Collider</c> instances into
        ///     the overlap buffer (<see cref="overlapBuffer"/>).
        /// </summary>
        public virtual void CheckObjectsInRegion()
        {
            if (regionCollider == null || overlapBuffer == null)
            {
                return;
            }

            // Reset number of found objects.
            objectsFound = 0;

            Transform regionTransform = regionCollider.transform;
            Vector3 lossyScale = regionTransform.lossyScale;

            // Perform non-allocating overlapping Collider search.
            if (regionCollider is BoxCollider box)
            {
                Vector3 halfExtents = Vector3.Scale(box.size, lossyScale) * 0.5f;

                objectsFound = Physics.OverlapBoxNonAlloc(regionTransform.TransformPoint(box.center), halfExtents, overlapBuffer,
                    regionTransform.rotation, layerMask, QueryTriggerInteraction.Collide);
            }
            else if (regionCollider is SphereCollider sphere)
            {
                float largestComponent = Math.Max(Math.Abs(lossyScale.x), Math.Max(Math.Abs(lossyScale.y), Math.Abs(lossyScale.z)));

                objectsFound = Physics.OverlapSphereNonAlloc(regionTransform.TransformPoint(sphere.center), sphere.radius * largestComponent,
                    overlapBuffer, layerMask, QueryTriggerInteraction.Collide);
            }
            else if (regionCollider is CapsuleCollider capsule)
            {
                lossyScale[capsule.direction] = 0.0f;

                float offset = (capsule.height * 0.5f) - capsule.radius;
                Vector3 direction = regionTransform.rotation * new Vector3() { [capsule.direction] = 1 } * offset;

                float largestComponent = Math.Max(Math.Abs(lossyScale.x), Math.Max(Math.Abs(lossyScale.y), Math.Abs(lossyScale.z)));

                objectsFound = Physics.OverlapCapsuleNonAlloc(regionTransform.TransformPoint(capsule.center - direction),
                    regionTransform.TransformPoint(capsule.center + direction), capsule.radius * largestComponent, overlapBuffer, layerMask,
                    QueryTriggerInteraction.Collide);
            }
            else
            {
                Plugin.Logger.LogWarning($"Unsupported Collider shape used for DetectRegion '{gameObject.name}'! It must be either a "
                    + "BoxCollider, SphereCollider, or CapsuleCollider to perform the query.");
                return;
            }
            // ...

            // Invoke event after the region is checked.
            onRegionChecked.Invoke(objectsFound);
        }

        /// <summary>
        ///     Perform script activation at the specified <c>ActivationTime</c>.
        /// </summary>
        /// <param name="activationTime"><c>ActivationTime</c> set for the script.</param>
        public virtual void PerformActivation(ActivationTime activationTime)
        {
            CheckObjectsInRegion();
        }

        /// <summary>
        ///     <c>DunGen</c> listener called when the Dungeon finishes generating.
        /// </summary>
        /// <param name="dungeon">Dungeon that just finished generating.</param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            ActivationSelf.OnDungeonComplete();
        }
    }
}