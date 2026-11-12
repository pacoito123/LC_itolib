using itolib.Interfaces;
using itolib.Patches;
using System;
using UnityEngine;

namespace itolib.Behaviours.Enemies
{
    /// <summary>
    ///     Allows restricting Vain Shroud spawning to begin at one or several specific nodes, instead of a random one.
    /// </summary>
    public sealed class MoldNodes : MonoBehaviour, ISeededScript<MoldNodes>
    {
        /// <inheritdoc/>
        public ISeededScript<MoldNodes> SeededSelf { get; }

        /// <summary>
        ///     Nodes for Vain Shrouds to use as their starting position (<c>moldStartPosition</c>).
        /// </summary>
        /// <remarks><b>NOTE:</b> Objects need to be actual nodes with the <c>OutsideAINode</c> tag.</remarks>
        [Header("Mold Nodes")]
        [Tooltip("Nodes for Vain Shrouds to use as their starting position ('moldStartPosition'). NOTE: Objects need to be actual nodes with the 'OutsideAINode' tag")]
        [SerializeField] private GameObject?[]? allowedMoldNodes;

        /// <summary>
        ///     Whether to randomize selected node for every landing or not, otherwise the previously selected one is used (like vanilla).
        /// </summary>
        [Tooltip("Whether to randomize selected node for every landing or not, otherwise the previously selected one is used (like vanilla).")]
        [SerializeField] private bool randomizeEveryLanding;

        /// <summary>
        ///     Cache already-cast <c>ISeededScript</c> instance, and set seed offset.
        /// </summary>
        private MoldNodes()
        {
            SeededSelf = this;

            ISeededScript<MoldNodes>.SeedOffset = 2017; // Random instance seed value used for Vain Shrouds.
        }

        /// <summary>
        ///     Subscribe to <c>GetOutsideAINodes</c> event.
        /// </summary>
        private void OnEnable()
        {
            RoundManagerPatches.OnGetOutsideAINodes += OverrideMoldPosition;
        }

        /// <summary>
        ///     Unsubscribe from <c>GetOutsideAINodes</c> event.
        /// </summary>
        private void OnDisable()
        {
            RoundManagerPatches.OnGetOutsideAINodes -= OverrideMoldPosition;
        }

        /// <summary>
        ///     Override <c>moldStartPosition</c> value for the current moon.
        /// </summary>
        /// <param name="outsideAINodes"></param>
        private void OverrideMoldPosition(GameObject[] outsideAINodes)
        {
            RoundManagerPatches.OnGetOutsideAINodes -= OverrideMoldPosition;

            SelectableLevel? currentLevel = (RoundManager.Instance != null) ? RoundManager.Instance.currentLevel : null;

            if (currentLevel == null || outsideAINodes == null)
            {
                return;
            }

            if (allowedMoldNodes == null || allowedMoldNodes.Length == 0)
            {
                Plugin.Logger.LogWarning($"No allowed Mold nodes defined for MoldNodes component in GameObject '{name}'.");

                return;
            }

            if (!randomizeEveryLanding && currentLevel.moldStartPosition != -1)
            {
                int currentMoldPositionIndex = currentLevel.moldStartPosition;

                if (currentMoldPositionIndex >= 0 && currentMoldPositionIndex < outsideAINodes.Length)
                {
                    if (Array.IndexOf(allowedMoldNodes, outsideAINodes[currentMoldPositionIndex]) != -1)
                    {
                        Plugin.Logger.LogDebug($"Node at index '{currentMoldPositionIndex}' is in the list of allowed Mold nodes!");

                        return;
                    }

                    Plugin.Logger.LogInfo($"Node at index '{currentMoldPositionIndex}' is not in the list of allowed Mold nodes. Selecting a different one...");
                }
            }

            int allowedMoldNodeIndex = SeededSelf.GetSeededRandom().Next(0, allowedMoldNodes.Length);

            GameObject? selectedNode = allowedMoldNodes[allowedMoldNodeIndex];

            if (selectedNode == null || !selectedNode.CompareTag("OutsideAINode"))
            {
                Plugin.Logger.LogWarning($"Mold node at index '{allowedMoldNodeIndex}' is missing or not tagged as 'OutsideAINode'.");

                return;
            }

            int outsideAINodeIndex = Array.IndexOf(outsideAINodes, selectedNode);

            if (outsideAINodeIndex == -1)
            {
                Plugin.Logger.LogWarning($"Mold node at index '{allowedMoldNodeIndex}' not found in the list of outside nodes.");

                return;
            }

            currentLevel.moldStartPosition = outsideAINodeIndex;
        }
    }
}