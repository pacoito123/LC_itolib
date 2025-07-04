using GameNetcodeStuff;
using itolib.Behaviours.Helpers;
using itolib.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace itolib.Behaviours.Effects
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class AttachedEffect : PooledObject<GameObject>
    {
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
        [Min(1)]
        public int maxInstances = 8;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        public int prepareInstances = 1;

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
        public UnityEvent<GameObject> onAttach = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public UnityEvent<GameObject> onDetach = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public Transform currentPosition = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public Transform targetPosition = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Awake()
        {
            currentPosition = transform;
            targetPosition = null!;

            base.Awake();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Update()
        {
            if (!followObject || targetPosition == null)
            {
                return;
            }

            currentPosition.position = targetPosition.position;
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
            if (PooledSelf.TryAssignInstance(gameObject, maxInstances, out IPooledObject<GameObject> instance))
            {
                if (instance is AttachedEffect effect)
                {
                    effect.currentPosition.position = effect.TakenBy.transform.position;
                    effect.targetPosition = effect.TakenBy.transform;

                    effect.onAttach.Invoke(gameObject);

                    effect.enabled = true;
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
            if (PooledSelf.TryFreeInstance(gameObject, out IPooledObject<GameObject> instance))
            {
                if (instance is AttachedEffect effect)
                {
                    effect.onDetach.Invoke(gameObject);
                    effect.targetPosition = null!;

                    effect.enabled = false;
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns> 
        public override IPooledObject<GameObject> CreateInstance()
        {
            AttachedEffect instance = Instantiate(effectToAttach, transform.GetParent(), false);
            instance.name = name;

            return instance;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void CreateInstances(int instances)
        {
            throw new System.NotImplementedException(); // TODO: Actually implement...
        }
    }
}