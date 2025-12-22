using DunGen;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public class OutOfBoundsAdjuster : MonoBehaviour, IDungeonCompleteReceiver
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        [Header("OutOfBounds Adjuster")]
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

            GameObject[]? rootObjects = SceneManager.GetSceneByName(StartOfRound.Instance.currentLevel.sceneName).GetRootGameObjects();

            for (int i = 0; i < rootObjects?.Length; i++)
            {
                OutOfBoundsTrigger? outOfBounds = rootObjects[i].GetComponentInChildren<OutOfBoundsTrigger>();

                if (outOfBounds != null)
                {
                    outOfBounds.transform.position = new(dungeon.transform.position.x + offsetToApply.x, dungeon.transform.position.y
                        + dungeon.Bounds.min.y + offsetToApply.y, dungeon.transform.position.z + offsetToApply.z);
                }
            }
        }
    }
}