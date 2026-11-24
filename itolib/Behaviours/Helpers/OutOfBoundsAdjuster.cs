using DunGen;
using itolib.Patches;
using System.Collections.Generic;
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
        private void OnDestroy()
        {
            OutOfBoundsPatch.currentOutOfBoundsHeight = float.MaxValue;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="dungeon"></param>
        public void OnDungeonComplete(Dungeon dungeon)
        {
            Scene currentScene = SceneManager.GetSceneByName((StartOfRound.Instance != null && StartOfRound.Instance.currentLevel != null)
                ? StartOfRound.Instance.currentLevel.sceneName : null);

            if (!currentScene.IsValid())
            {
                return;
            }

            float lowestPoint = dungeon.Bounds.min.y;

            HashSet<OutOfBoundsTrigger> allTriggers = [];
            List<OutOfBoundsTrigger> tempTriggers = [];

            foreach (GameObject rootObject in currentScene.GetRootGameObjects())
            {
                rootObject.GetComponentsInChildren(includeInactive: true, tempTriggers);
                allTriggers.UnionWith(tempTriggers);

                foreach (OutOfBoundsTrigger trigger in tempTriggers)
                {
                    float currentY = trigger.transform.position.y;

                    if (currentY < lowestPoint)
                    {
                        lowestPoint = currentY;
                    }
                }
            }

            foreach (OutOfBoundsTrigger trigger in allTriggers)
            {
                trigger.transform.position = new Vector3(trigger.transform.position.x, lowestPoint, trigger.transform.position.z) + offsetToApply;
            }

            OutOfBoundsPatch.currentOutOfBoundsHeight = lowestPoint + offsetToApply.y - 225.0f;
        }
    }
}