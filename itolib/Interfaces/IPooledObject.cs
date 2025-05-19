using UnityEngine;

namespace itolib.Interfaces
{
    /// <summary>
    ///     Defines an <c>Object</c> with object pooling capabilities, which also is its own <c>LinkedList</c> that holds all its instances.
    ///     Contains some default implementations for assigning and freeing instances.
    /// </summary>
    /// <remarks>Not me only finding out 'UnityEngine.Pool.IObjectPool' exists right after implementing this... 💀</remarks>
    public interface IPooledObject
    {
        /// <summary>
        ///     Instance number for the <c>Object</c>'s current pool. Intended for tracking and/or limiting the amount of created instances.
        /// </summary>
        int ObjectID { get; set; }

        /// <summary>
        ///     External <c>GameObject</c> that has taken 'ownership' of this pooled object instance.
        /// </summary>
        GameObject TakenBy { get; set; }

        /// <summary>
        ///     Next pooled object instance in the <c>LinkedList</c>.
        /// </summary>
        IPooledObject Next { get; set; }

        /// <summary>
        ///     Instantiates a new pooled object instance.
        /// </summary>
        /// <remarks>Intended for doing any kind of extra initialization, or for setting additional properties.</remarks>
        /// <returns>The newly-created pooled object instance.</returns>
        IPooledObject CreateInstance();

        /// <summary>
        ///     Attempt to assign a given <c>GameObject</c> to a new or existing pooled object instance.
        /// </summary>
        /// <param name="taker">External <c>GameObject</c> attempting to grab a pooled object instance.</param>
        /// <param name="maxInstances">Maximum number of pooled object instances that can be created.</param>
        /// <param name="pooledObject">The assigned pooled object instance, if successful.</param>
        /// <returns>Whether a pooled object instance was successfully assigned or not.</returns>
        bool TryAssignInstance(GameObject taker, int maxInstances, out IPooledObject pooledObject)
        {
            pooledObject = null!;

            if (TakenBy == null)
            {
                // Use the current (ownerless) pooled object instance.
                pooledObject = this;
            }
            else if (Next == null && ObjectID < maxInstances)
            {
                // Create a new pooled object instance, if not exceeding the maximum number of instances.
                Next = CreateInstance();
                Next.ObjectID = ObjectID + 1;

                // Use the newly-created pooled object instance.
                pooledObject = Next;
            }

            if (pooledObject != null)
            {
                pooledObject.TakenBy = taker;

                // Enable behaviour script for the assigned pooled object instance.
                MonoBehaviour? behaviour = pooledObject as MonoBehaviour;
                if (behaviour != null && !behaviour.enabled)
                {
                    behaviour.enabled = true;
                }

                return true;
            }

            // Attempt to assign to the next linked pooled object instance, or return false if there are no more instances left.
            return Next?.TryAssignInstance(taker, maxInstances, out pooledObject) ?? false;
        }

        /// <summary>
        ///     Check if a given <c>GameObject</c> is assigned to a pooled object instance, and free that instance to be reused.
        /// </summary>
        /// <param name="possibleOwner">External <c>GameObject</c> that might be assigned to a pooled object instance.</param>
        /// <param name="pooledObject">The now-freed pooled object instance, if successful.</param>
        /// <returns>Whether a pooled object instance was successfully freed or not.</returns>
        bool TryFreeInstance(GameObject possibleOwner, out IPooledObject pooledObject)
        {
            pooledObject = null!;

            // Check if the possible owner is assigned to the current pooled object instance.
            if (ReferenceEquals(possibleOwner, TakenBy))
            {
                pooledObject = this;
                TakenBy = null!;

                // Disable behaviour script for the freed pooled object instance.
                MonoBehaviour? behaviour = this as MonoBehaviour;
                if (behaviour != null && behaviour.enabled)
                {
                    behaviour.enabled = false;
                }

                return true;
            }

            // Check next linked pooled object instance, or return false if there are no more instances left.
            return Next?.TryFreeInstance(possibleOwner, out pooledObject) ?? false;
        }
    }
}