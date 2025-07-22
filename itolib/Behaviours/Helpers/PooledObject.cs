using itolib.Interfaces;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     Implementation of <c>IPooledObject</c> for Unity objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PooledObject<T> : MonoBehaviour, IPooledObject<T> where T : Object
    {
        /// <summary>
        ///     Instance number for the <c>PooledObject</c>'s current pool. Intended for tracking and/or limiting the amount of created instances.
        /// </summary>
        public int ObjectID { get; set; }

        /// <summary>
        ///     External <c><typeparamref name="T"/></c> that has taken 'ownership' of this <c>PooledObject</c> instance.
        /// </summary>
        public T TakenBy { get; set; } = default!;

        /// <summary>
        ///     Next <c>PooledObject</c> instance in the pool.
        /// </summary>
        public IPooledObject<T> NextPooledObject { get; set; } = null!;

        /// <summary>
        ///     Maximum number of instances that can be created for this <c>PooledObject</c>.
        /// </summary>
        [Header("Pooled Object")]
        [Tooltip("Maximum number of instances that can be created for this PooledObject.")]
        [Min(1)]
        [SerializeField] protected int maxInstances = 8;

        /// <summary>
        ///     Number of <c>PooledObject</c> instances to prepare and have ready from the start.
        /// </summary>
        [Tooltip("Number of PooledObject instances to prepare and have ready from the start.")]
        [Min(0)]
        [SerializeField] protected int createInstances = 4;

        /// <summary>
        ///     Cached instance of the current <c>PooledObject</c> as an <c>IPooledObject</c>, to avoid having to cast. 
        /// </summary>
        protected IPooledObject<T> pooledSelf = default!;

        /// <summary>
        ///     Cache already-cast <c>IPooledObject</c> instance.
        /// </summary>
        protected virtual void Awake()
        {
            pooledSelf = this;
        }

        /// <summary>
        ///     Begin preparing <c>PooledObject</c> instances (if the current object is the root).
        /// </summary>
        private void Start()
        {
            if (ObjectID == 0 && createInstances > 0)
            {
                pooledSelf.PrepareInstances(createInstances - 1);
            }

            // Start disabled, until being assigned to an object.
            enabled = false;
        }

        /// <summary>
        ///     Instantiates a new <c>IPooledObject</c> instance.
        /// </summary>
        /// <returns>The newly-created <c>IPooledObject</c> instance.</returns>
        public abstract IPooledObject<T> CreateInstance();
    }
}