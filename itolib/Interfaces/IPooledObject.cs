namespace itolib.Interfaces
{
    /// <summary>
    ///     Adds object pooling capabilities to any implementing class, which also becomes its own <c>LinkedList</c> that holds all its instances.
    ///     Contains some default implementations for assigning and freeing instances.
    /// </summary>
    /// <typeparam name="T">The type of the object taking ownership of this pooled object instance.</typeparam>
    /// <remarks>Not me only finding out <c>UnityEngine.Pool.IObjectPool</c> exists right after implementing this... 💀</remarks>
    public interface IPooledObject<T>
    {
        /// <summary>
        ///     Instance number for the current object in the pool. Intended for tracking and/or limiting the amount of created instances.
        /// </summary>
        int ObjectID { get; set; }

        /// <summary>
        ///     External <c><typeparamref name="T"/></c> that has taken 'ownership' of this pooled object instance.
        /// </summary>
        T TakenBy { get; set; }

        /// <summary>
        ///     Next pooled object instance in the <c>LinkedList</c>.
        /// </summary>
        IPooledObject<T> NextPooledObject { get; set; }

        /// <summary>
        ///     Instantiates a new pooled object instance.
        /// </summary>
        /// <remarks>Intended for doing any kind of extra initialization, or for setting additional properties.</remarks>
        /// <returns>The newly-created pooled object instance.</returns>
        IPooledObject<T> CreateInstance();

        /// <summary>
        ///     Instantiates multiple instances of the pooled object, to have prepared.
        /// </summary>
        /// <param name="instances">The number of instances to create.</param>
        void CreateInstances(int instances);

        /// <summary>
        ///     Attempt to assign a given <c><typeparamref name="T"/></c> to a new or existing pooled object instance.
        /// </summary>
        /// <param name="taker">External <c><typeparamref name="T"/></c> attempting to grab a pooled object instance.</param>
        /// <param name="maxInstances">Maximum number of pooled object instances that can be created.</param>
        /// <param name="pooledObject">The assigned pooled object instance, if successful.</param>
        /// <returns>Whether a pooled object instance was successfully assigned or not.</returns>
        bool TryAssignInstance(T taker, int maxInstances, out IPooledObject<T> pooledObject)
        {
            pooledObject = null!;

            if (TakenBy == null)
            {
                // Use the current (ownerless) pooled object instance.
                pooledObject = this;
            }
            else if (NextPooledObject == null && ObjectID < maxInstances)
            {
                // Create a new pooled object instance, if not exceeding the maximum number of instances.
                NextPooledObject = CreateInstance();
                NextPooledObject.ObjectID = ObjectID + 1;

                // Use the newly-created pooled object instance.
                pooledObject = NextPooledObject;
            }

            if (pooledObject != null)
            {
                // Assign taker as owner of this pooled object instance.
                pooledObject.TakenBy = taker;

                return true;
            }

            // Attempt to assign to the next linked pooled object instance, or return false if there are no more instances left.
            return NextPooledObject != null && NextPooledObject.TryAssignInstance(taker, maxInstances, out pooledObject);
        }

        /// <summary>
        ///     Check if a given <c><typeparamref name="T"/></c> is assigned to a pooled object instance, and free that instance to be reused.
        /// </summary>
        /// <param name="possibleOwner">External <c><typeparamref name="T"/></c> that might be assigned to a pooled object instance.</param>
        /// <param name="pooledObject">The now-freed pooled object instance, if successful.</param>
        /// <returns>Whether a pooled object instance was successfully freed or not.</returns>
        bool TryFreeInstance(T possibleOwner, out IPooledObject<T> pooledObject)
        {
            pooledObject = null!;

            // Check if the possible owner is assigned to the current pooled object instance.
            if (ReferenceEquals(possibleOwner, TakenBy))
            {
                pooledObject = this;
                TakenBy = default!;

                return true;
            }

            // Check next linked pooled object instance, or return false if there are no more instances left.
            return NextPooledObject != null && NextPooledObject.TryFreeInstance(possibleOwner, out pooledObject);
        }
    }
}