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
        [SerializeField] private AttachedEffect effectToAttach = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(1)]
        [SerializeField] private int maxInstances = 8;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [Min(0)]
        [SerializeField] private int prepareInstances = 1;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private bool followObject = true;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        [SerializeField] private UnityEvent<GameObject> onAttach = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        [SerializeField] private UnityEvent<GameObject> onDetach = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        private Transform currentPosition = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Transform targetPosition = null!;

        /// <summary>
        ///     TODO.
        /// </summary>
        protected override void Awake()
        {
            currentPosition = transform;
            targetPosition = null!;

            base.Awake();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Update()
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
        /// <param name="player"></param>
        public void AttachPlayer(PlayerControllerB player)
        {
            Attach(player.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public void AttachEnemy(EnemyAI enemy)
        {
            Attach(enemy.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void AttachItem(GrabbableObject item)
        {
            Attach(item.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="gameObject"></param>
        public void Attach(GameObject gameObject)
        {
            if (pooledSelf.TryAssignInstance(gameObject, maxInstances, out IPooledObject<GameObject> instance))
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
        /// <param name="player"></param>
        public void DetachPlayer(PlayerControllerB player)
        {
            Detach(player.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public void DetachEnemy(EnemyAI enemy)
        {
            Detach(enemy.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void DetachItem(GrabbableObject item)
        {
            Detach(item.gameObject);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="gameObject"></param>
        public void Detach(GameObject gameObject)
        {
            if (pooledSelf.TryFreeInstance(gameObject, out IPooledObject<GameObject> instance))
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
            // TODO: Actually implement...
        }
    }
}