using LethalLevelLoader;
using Unity.Netcode;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     Activates the Apparatus if it's spawned in the middle of a round, also assigns Old Bird enemy type.
    /// </summary>
    public class ActivateApparatus : NetworkBehaviour
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        public static EnemyType? OldBirdEnemyType
        {
            get
            {
                if (field == null)
                {
                    OldBirdEnemyType = OriginalContent.Enemies.Find(enemy => string.CompareOrdinal(enemy.enemyName, "RadMech") == 0);
                }

                return field;
            }
            private set;
        }

        private void Start()
        {
            if (IsHost)
            {
                Activate();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if (!IsHost)
            {
                Activate();
            }

            base.OnNetworkSpawn();
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        public void Activate()
        {
            if (TryGetComponent(out LungProp apparatus) && !apparatus.isInShipRoom
                && TryGetComponent(out AudioSource apparatusSource))
            {
                apparatus.isLungDocked = true;
                apparatus.isLungPowered = true;

                apparatus.radMechEnemyType = OldBirdEnemyType;

                apparatusSource.loop = true;
                apparatusSource.Play();
            }
        }
    }
}