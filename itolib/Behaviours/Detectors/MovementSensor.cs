using GameNetcodeStuff;
using itolib.Behaviours.Effects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace itolib.Behaviours.Detectors
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class MovementSensor : PlayerAttachable
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public float Timer { get; private set; } = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public InputAction? ActionToTrigger { get; private set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Movement Sensor")]
        [Tooltip("")]
        public float triggerInterval = 1.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        [Tooltip("")]
        public string actionToTrigger = "Move";

        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Events")]
        [Tooltip("")]
        public UnityEvent<PlayerControllerB>? onMovementDetected;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            AttachCondition = player => !player.isPlayerDead;
            DetachCondition = player => player.isPlayerDead;

            if (actionToTrigger.Length > 0)
            {
                // Get action (or key) that triggers this sensor.
                ActionToTrigger = GameNetworkManager.Instance.localPlayerController.playerActions.m_Movement.FindAction(actionToTrigger);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void OnEnable()
        {
            if (ActionToTrigger == null)
            {
                enabled = false;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (AttachedPlayer == null)
            {
                Timer = triggerInterval;
                return;
            }

            if (Timer < triggerInterval)
            {
                Timer += Time.deltaTime;
                return;
            }

            if (!ActionToTrigger!.IsPressed())
            {
                return;
            }

            onMovementDetected?.Invoke(AttachedPlayer);
            Timer = 0.0f;
        }
    }
}