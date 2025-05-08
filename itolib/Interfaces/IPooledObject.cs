using UnityEngine;

namespace itolib.Interfaces
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <remarks>Not me only finding out 'UnityEngine.Pool.IObjectPool' exists right after implementing this... 💀</remarks>
    public interface IPooledObject
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        int ObjectID { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        GameObject TakenBy { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        IPooledObject Next { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        IPooledObject CreateInstance();

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="maxInstances"></param>
        /// <param name="pooledObject"></param>
        /// <returns></returns>
        bool RequestInstance(int maxInstances, out IPooledObject pooledObject)
        {
            pooledObject = null!;

            if (TakenBy == null)
            {
                pooledObject = this;

                return true;
            }

            if (Next != null)
            {
                return Next.RequestInstance(maxInstances, out pooledObject);
            }
            else if (ObjectID < maxInstances)
            {
                Next = CreateInstance();
                Next.ObjectID = ObjectID + 1;

                pooledObject = Next;

                return true;
            }

            return false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="taker"></param>
        /// <param name="maxInstances"></param>
        /// <returns></returns>
        bool TryAssignInstance(GameObject taker, int maxInstances)
        {
            if (RequestInstance(maxInstances, out IPooledObject instance))
            {
                instance.TakenBy = taker;

                MonoBehaviour? behaviour = instance as MonoBehaviour;
                if (behaviour != null && !behaviour.enabled)
                {
                    behaviour.enabled = true;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="possibleOwner"></param>
        /// <returns></returns>
        bool TryFreeInstance(GameObject possibleOwner)
        {
            if (possibleOwner.GetInstanceID() == TakenBy?.GetInstanceID())
            {
                TakenBy = null!;

                MonoBehaviour? behaviour = this as MonoBehaviour;
                if (behaviour != null && !behaviour.enabled)
                {
                    behaviour.enabled = false;
                }

                return true;
            }

            return Next?.TryFreeInstance(possibleOwner) ?? false;
        }
    }
}