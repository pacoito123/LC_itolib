using itolib.Extensions;
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
                    field = OriginalContent.Enemies.Find(enemy => enemy.enemyName.CompareOrdinal("RadMech"));
                }

                return field;
            }
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
            base.OnNetworkSpawn();

            if (!IsHost)
            {
                Activate();
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private void Activate()
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