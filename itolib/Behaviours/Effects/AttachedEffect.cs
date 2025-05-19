using GameNetcodeStuff;
using itolib.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AttachedEffect : MonoBehaviour, IPooledObject
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public int ObjectID { get; set; } = 0;

        /// <summary>
        ///     TODO.
        /// </summary>
        public GameObject TakenBy { get; set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public IPooledObject Next { get; set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Transform CurrentPosition { get; private set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Transform TargetPosition { get; private set; } = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Attached Effect")]
        [Tooltip("")]
        public AttachedEffect effectToAttach = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public int maxInstances = 8;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public bool followObject = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent<GameObject>? onAttach;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<GameObject>? onDetach;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            CurrentPosition = transform;

            TakenBy = null!;
            Next = null!;
            TargetPosition = null!;

            enabled = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Update()
        {
            if (!followObject || TargetPosition == null)
            {
                return;
            }

            CurrentPosition.position = TargetPosition.position;
            // CurrentPosition.SetPositionAndRotation(TargetPosition.position, TargetPosition.rotation);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void AttachPlayer(PlayerControllerB player)
        {
            Attach(player.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void AttachEnemy(EnemyAI enemy)
        {
            Attach(enemy.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="gameObject"></param>
        public void Attach(GameObject gameObject)
        {
            if ((this as IPooledObject).TryAssignInstance(gameObject, maxInstances, out IPooledObject instance))
            {
                if (instance is AttachedEffect effect)
                {
                    effect.CurrentPosition.position = effect.TakenBy.transform.position;
                    effect.TargetPosition = effect.TakenBy.transform;

                    effect.onAttach?.Invoke(gameObject);
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void DetachPlayer(PlayerControllerB player)
        {
            Detach(player.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void DetachEnemy(EnemyAI enemy)
        {
            Detach(enemy.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="gameObject"></param>
        public void Detach(GameObject gameObject)
        {
            if ((this as IPooledObject).TryFreeInstance(gameObject, out IPooledObject instance))
            {
                if (instance is AttachedEffect effect)
                {
                    effect.onDetach?.Invoke(gameObject);
                    effect.TargetPosition = null!;
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns> 
        public IPooledObject CreateInstance()
        {
            AttachedEffect instance = Instantiate(effectToAttach, transform.GetParent(), false);
            instance.name = name;

            return instance;
        }
    }
}