using DunGen;
using itolib.Patches;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     Moves a moon's <c>OutOfBoundsTrigger</c> to the lowest point in the current <c>Dungeon</c> generation, with an optional extra offset.
    /// </summary>
    public class OutOfBoundsAdjuster : MonoBehaviour
    {
        /// <summary>
        ///     Additional offset to apply to the <c>OutOfBoundsTrigger</c>.
        /// </summary>
        [Header("OutOfBounds Adjuster")]
        [Tooltip("Additional offset to apply to the out of bounds trigger.")]
        [SerializeField] private Vector3 offsetToApply = Vector3.zero;

        /// <summary>
        ///     Subscribe to <c>Dungeon</c> generation complete event.
        /// </summary>
        private void OnEnable()
        {
            if (RoundManager.Instance == null || RoundManager.Instance.dungeonGenerator == null)
            {
                return;
            }

            RoundManager.Instance.dungeonGenerator.Generator?.OnGenerationStatusChanged += MoveOutOfBoundsTrigger;
        }

        /// <summary>
        ///     Unsubscribe from <c>Dungeon</c> generation complete event, and reset current fatal height.
        /// </summary>
        private void OnDisable()
        {
            if (RoundManager.Instance == null || RoundManager.Instance.dungeonGenerator == null)
            {
                return;
            }

            RoundManager.Instance.dungeonGenerator.Generator?.OnGenerationStatusChanged -= MoveOutOfBoundsTrigger;

            OutOfBoundsPatch.currentOutOfBoundsHeight = float.MaxValue; // Gets reset to '-600' at the end of the frame.
        }

        /// <summary>
        ///     Move <c>OutOfBoundsTrigger</c> once the <c>Dungeon</c> finishes generating.
        /// <param name="generator"><c>DungeonGenerator</c> currently generating the <c>Dungeon</c>.</param>
        /// <param name="status">Current <c>Dungeon</c> generation step.</param>
        /// </summary>
        private void MoveOutOfBoundsTrigger(DungeonGenerator generator, GenerationStatus status)
        {
            // Check if Dungeon has not yet finished generating.
            if (status is not GenerationStatus.Complete)
            {
                return;
            }

            // Check if current Dungeon exists.
            if (generator == null || generator.CurrentDungeon == null)
            {
                return;
            }

            // Obtain currently loaded moon scene.
            Scene currentScene = SceneManager.GetSceneByName((StartOfRound.Instance != null && StartOfRound.Instance.currentLevel != null)
                ? StartOfRound.Instance.currentLevel.sceneName : null);

            // Check if scene obtained is valid.
            if (!currentScene.IsValid())
            {
                return;
            }

            // Obtain lowest point in the current Dungeon generation.
            float lowestPoint = generator.CurrentDungeon.Bounds.min.y;

            HashSet<OutOfBoundsTrigger> allTriggers = [];
            List<OutOfBoundsTrigger> tempTriggers = [];

            // Check every object in the root of the current scene.
            foreach (GameObject rootObject in currentScene.GetRootGameObjects())
            {
                // Obtain all OutOfBoundsTrigger components in the current root object's hierarchy.
                rootObject.GetComponentsInChildren(includeInactive: true, tempTriggers);
                allTriggers.UnionWith(tempTriggers);

                // Check each found OutOfBoundsTrigger for the lowest point.
                foreach (OutOfBoundsTrigger trigger in tempTriggers)
                {
                    float currentY = trigger.transform.position.y;

                    // Check if found OutOfBoundsTrigger is below the current lowest point.
                    if (currentY < lowestPoint)
                    {
                        lowestPoint = currentY;
                    }
                }
            }

            // Move every OutOfBoundsTrigger in the current scene to the lowest point, and apply offset.
            foreach (OutOfBoundsTrigger trigger in allTriggers)
            {
                trigger.transform.position = new Vector3(trigger.transform.position.x, lowestPoint, trigger.transform.position.z) + offsetToApply;
            }

            // Set fatal height to 225 units below the OutOfBoundsTrigger (like vanilla).
            OutOfBoundsPatch.currentOutOfBoundsHeight = lowestPoint + offsetToApply.y - 225.0f;
        }
    }
}