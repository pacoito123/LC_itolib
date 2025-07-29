using DunGen;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class CeilingAdjuster : MonoBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("Ceiling Adjuster")]
        [Tooltip("")]
        [SerializeField] private Vector3 offsetToApply = Vector3.zero;

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            if (StartOfRound.Instance == null || StartOfRound.Instance.currentLevel == null)
            {
                return;
            }

            transform.position = new(dungeon.transform.position.x + offsetToApply.x, dungeon.transform.position.y
                + dungeon.Bounds.max.y + offsetToApply.y, dungeon.transform.position.z + offsetToApply.z);
        }
    }
}