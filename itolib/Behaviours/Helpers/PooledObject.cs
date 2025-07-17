using itolib.Interfaces;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PooledObject<T> : MonoBehaviour, IPooledObject<T> where T : Object
    {
        /// <summary>
        ///     Instance number for the <c>PooledObject</c>'s current pool. Intended for tracking and/or limiting the amount of created instances.
        /// </summary>
        public int ObjectID { get; set; }

        /// <summary>
        ///     External <c><typeparamref name="T"/></c> that has taken 'ownership' of this pooled object instance.
        /// </summary>
        public T TakenBy { get; set; } = default!;

        /// <summary>
        ///     Next pooled object instance in the linked list.
        /// </summary>
        public IPooledObject<T> NextPooledObject { get; set; } = null!;

        /// <summary>
        ///     Cached instance of the current object as an <c>IPooledObject</c>, to avoid having to cast. 
        /// </summary>
        protected IPooledObject<T> pooledSelf = default!;

        /// <summary>
        ///     Initialize some fields.
        /// </summary>
        protected virtual void Awake()
        {
            pooledSelf = this;

            enabled = false;
        }

        /// <summary>
        ///     Instantiates a new pooled object instance.
        /// </summary>
        /// <returns>The newly-created pooled object instance.</returns>
        public abstract IPooledObject<T> CreateInstance();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="instances"></param>
        public abstract void CreateInstances(int instances);
    }
}