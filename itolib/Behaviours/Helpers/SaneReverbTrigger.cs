using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class SaneReverbTrigger : AudioReverbTrigger
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public float delayCheck;

        /// <summary>
        ///     TODO.
        /// </summary>
        public bool onlyOnEnter;

        /// <summary>
        ///     TODO.
        /// </summary>
        [HideInInspector]
        public float timeSinceLastCheck;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            timeSinceLastCheck = delayCheck;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerEnter(Collider other)
        {
            if (onlyOnEnter)
            {
                base.OnTriggerStay(other);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public new void OnTriggerStay(Collider other)
        {
            if (onlyOnEnter)
            {
                return;
            }

            if (delayCheck == 0)
            {
                base.OnTriggerStay(other);

                return;
            }

            if (timeSinceLastCheck <= delayCheck)
            {
                timeSinceLastCheck += Time.deltaTime;

                return;
            }

            timeSinceLastCheck = 0.0f;

            base.OnTriggerStay(other);
        }
    }
}