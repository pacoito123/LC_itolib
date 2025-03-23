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
        public float TimeSinceLastCheck { get; private set; } = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public float delayCheck = 0.0f;

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Awake()
        {
            TimeSinceLastCheck = delayCheck;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public new void OnTriggerStay(Collider other)
        {
            if (delayCheck == 0)
            {
                base.OnTriggerStay(other);

                return;
            }

            if (TimeSinceLastCheck <= delayCheck)
            {
                TimeSinceLastCheck += Time.deltaTime;

                return;
            }

            TimeSinceLastCheck = 0.0f;

            base.OnTriggerStay(other);
        }

        /* /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerEnter(Collider other)
        {
            if (elevatorTriggerForProps)
            {
                if (setInElevatorTrigger && other.CompareTag("Enemy") && TryGetComponent(out Collider enemy)
                    && enemy.bounds.Contains(other.transform.position))
                {
                    if (other.TryGetComponent(out EnemyAICollisionDetect collisionDetect))
                    {
                        collisionDetect.mainScript.isInsidePlayerShip = isShipRoom;

                        if (collisionDetect.mainScript.isInsidePlayerShip != isShipRoom)
                        {
                            StartOfRound.Instance.SetPlayerSafeInShip();
                        }
                    }

                    return;
                }

                if (other.tag.StartsWith("PlayerRagdoll") && other.TryGetComponent(out DeadBodyInfo ragdoll))
                {
                    if (ragdoll.attachedTo != null && ragdoll.attachedLimb != null)
                    {
                        return;
                    }

                    ragdoll.parentedToShip = setInElevatorTrigger;
                    if (ragdoll.attachedLimb == null || ragdoll.attachedTo == null)
                    {
                        if (setInElevatorTrigger)
                        {
                            ragdoll.transform.SetParent(StartOfRound.Instance.elevatorTransform);
                        }
                        else
                        {
                            ragdoll.transform.SetParent(null);
                        }
                    }
                }
            }

            if (other.gameObject.CompareTag("Player") && !(GameNetworkManager.Instance.localPlayerController == null))
            {
                playerScript = other.gameObject.GetComponent<PlayerControllerB>();
                if (!(playerScript == null) && playerScript.isPlayerControlled)
                {
                    ChangeAudioReverbForPlayer(playerScript);
                }
            }
        } */
    }
}