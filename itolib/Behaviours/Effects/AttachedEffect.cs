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
        public AttachedEffect EffectToAttach { get; private set; } = null!;

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
            EffectToAttach = this;
            CurrentPosition = transform;

            enabled = false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Update()
        {
            if (!followObject)
            {
                return;
            }

            if (TakenBy != null)
            {
                if (TargetPosition == null)
                {
                    TargetPosition = TakenBy.transform;
                }

                CurrentPosition.position = TargetPosition.position;
                // CurrentPosition.SetPositionAndRotation(TargetPosition.position, TargetPosition.rotation);
            }
            else
            {
                TargetPosition = null!;
            }
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
            if ((this as IPooledObject).TryAssignInstance(gameObject, maxInstances))
            {
                onAttach?.Invoke(gameObject);
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
            if ((this as IPooledObject).TryFreeInstance(gameObject))
            {
                onDetach?.Invoke(gameObject);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns> 
        public IPooledObject CreateInstance()
        {
            AttachedEffect instance = Instantiate(EffectToAttach, transform.GetParent(), false);
            instance.name = name;

            return instance;
        }
    }
}