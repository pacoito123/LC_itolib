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
    public class AttachedEffect : PooledObject<Collider>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Space(5.0f)]
        [Header("Attached Effect")]
        [Tooltip("")]
        [SerializeField] private AttachedEffect effectToAttach = null!;

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
        private Transform currentPosition;

        /// <summary>
        ///     TODO.
        /// </summary>
        private Transform targetPosition;

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
            if (TakenBy == null)
            {
                TakenBy = null!; // Needed for destroyed objects...

                enabled = false;

                return;
            }

            if (!TakenBy.enabled)
            {
                Detach(TakenBy);

                return;
            }

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
            Attach(player.GetComponent<Collider>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public void AttachEnemy(EnemyAI enemy)
        {
            Attach(enemy.GetComponent<Collider>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void AttachItem(GrabbableObject item)
        {
            Attach(item.GetComponent<Collider>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="collider"></param>
        public void Attach(Collider collider)
        {
            if (collider == null || !collider.enabled)
            {
                return;
            }

            if (pooledSelf.TryAssignInstance(collider, maxInstances - 1, out IPooledObject<Collider> instance))
            {
                if (instance is AttachedEffect effect)
                {
                    effect.targetPosition = effect.TakenBy.transform;
                    effect.currentPosition.position = effect.targetPosition.position;

                    effect.onAttach.Invoke(collider.gameObject);

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
            Detach(player.GetComponent<Collider>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="enemy"></param>
        public void DetachEnemy(EnemyAI enemy)
        {
            Detach(enemy.GetComponent<Collider>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="item"></param>
        public void DetachItem(GrabbableObject item)
        {
            Detach(item.GetComponent<Collider>());
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="collider"></param>
        public void Detach(Collider collider)
        {
            if (collider == null)
            {
                return;
            }

            if (pooledSelf.TryFreeInstance(collider, out IPooledObject<Collider> instance))
            {
                if (instance is AttachedEffect effect)
                {
                    effect.onDetach.Invoke(collider.gameObject);
                    effect.targetPosition = null!;

                    effect.enabled = false;
                }
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <returns></returns>
        public override IPooledObject<Collider> CreateInstance()
        {
            AttachedEffect instance = Instantiate(effectToAttach, transform.GetParent(), false);
            instance.name = name;

            return instance;
        }
    }
}