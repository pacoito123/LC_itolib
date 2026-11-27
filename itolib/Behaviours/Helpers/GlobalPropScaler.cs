using DunGen;
using DunGen.Graph;
using itolib.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace itolib.Behaviours.Helpers
{
    /// <summary>
    ///     Represents a modifier to apply to a specified <c>GlobalProp</c>.
    /// </summary>
    [Serializable]
    public struct GlobalPropModifier()
    {
        /// <summary>
        ///     ID corresponding to the <c>GlobalProp</c> to be modified.
        /// </summary>
        [field: Header("Global Prop Modifier")]
        [field: Tooltip("ID corresponding to the Global Prop to be modified.")]
        [field: Min(0)]
        [field: SerializeField] public int GlobalPropID { get; private set; }

        /// <summary>
        ///     Scale factor to apply to the lower bounds of the target <c>GlobalProp</c> count.
        /// </summary>
        /// <remarks><c>0</c> is unchanged, <c>1</c> equals the <c>Dungeon</c> size multiplier, <c>0.5</c> is a value in between those two, and values above <c>1</c> scale similarly.</remarks>
        [field: Tooltip("Scale factor to apply to the lower bounds of the target Global Prop count range. '0' is unchanged, '1' equals the Dungeon size multiplier, "
            + "'0.5' is a value in between those two, and values above '1' scale similarly.")]
        [field: Range(0.0f, 10.0f)]
        [field: SerializeField] public float MinScaleFactor { get; private set; }

        /// <summary>
        ///     Scale factor to apply to the upper bounds of the target <c>GlobalProp</c> count.
        /// </summary>
        /// <remarks><c>0</c> is unchanged, <c>1</c> equals the <c>Dungeon</c> size multiplier, <c>0.5</c> is a value in between those two, and values above <c>1</c> scale similarly.</remarks>
        [field: Tooltip("Scale factor to apply to the upper bounds of the target Global Prop count range. '0' is unchanged, '1' equals the Dungeon size multiplier, "
            + "'0.5' is a value in between those two, and values above '1' scale similarly.")]
        [field: Range(0.0f, 10.0f)]
        [field: SerializeField] public float MaxScaleFactor { get; private set; }

        /// <summary>
        ///     Type of operation to be performed on the target <c>GlobalProp</c> count range.
        /// </summary>
        [field: Tooltip("Type of operation to be performed on the target Global Prop count range.")]
        [field: SerializeField] public ModifierType ModifierType { get; private set; }

        /// <summary>
        ///     Apply modifier values to the given <c>GlobalProp</c> count range.
        /// </summary>
        /// <param name="count">Count range to modify.</param>
        /// <param name="dungeonSize">Size multiplier for the current <c>Dungeon</c>.</param>
        public readonly void ApplyModifier(IntRange? count, float dungeonSize)
        {
            if (count == null)
            {
                return;
            }

            ApplyModifier(ref count.Min, ModifierType, dungeonSize, MinScaleFactor);
            ApplyModifier(ref count.Max, ModifierType, dungeonSize, MaxScaleFactor);

            if (count.Min >= count.Max)
            {
                count.Max = count.Min;
            }
        }

        /// <summary>
        ///     Apply modifier to the given <c>GlobalProp</c> count value.
        /// </summary>
        /// <param name="value">Count value to modify.</param>
        /// <param name="modifierType">Type of operation to perform.</param>
        /// <param name="dungeonSize">Size multiplier for the current <c>Dungeon</c>.</param>
        /// <param name="scaleFactor">Scale factor to apply.</param>
        private static void ApplyModifier(ref int value, ModifierType modifierType, float dungeonSize, float scaleFactor)
        {
            value = modifierType switch
            {
                ModifierType.Additive => Mathf.RoundToInt(value + Mathf.LerpUnclamped(0.0f, dungeonSize, scaleFactor)),
                ModifierType.Multiplicative => Mathf.RoundToInt(value * Math.Max(Mathf.LerpUnclamped(1.0f, dungeonSize, scaleFactor), 0.0f)),
                ModifierType.Divisive => Mathf.RoundToInt(value / Math.Max(Mathf.LerpUnclamped(1.0f, dungeonSize, scaleFactor), Mathf.Epsilon)),
                _ => 0,
            };
        }
    }

    /// <summary>
    ///     Allows scaling one or several <c>GlobalProp</c> counts depending on the current <c>Dungeon</c> size multiplier.
    /// </summary>
    public sealed class GlobalPropScaler : MonoBehaviour
    {
        /// <summary>
        ///     Original values for <c>GlobalProp</c> counts, before any modifiers are applied.
        /// </summary>
        public static Dictionary<int, IntRange> OriginalValues { get; } = [];

        /// <summary>
        ///     List of <c>GlobalProp</c> modifiers to apply.
        /// </summary>
        [Header("Global Prop Scaler")]
        [Tooltip("List of Global Prop modifiers to apply.")]
        [SerializeField] private GlobalPropModifier[]? globalPropModifiers;

        /// <summary>
        ///     Subscribe to <c>Dungeon</c> generation state change event.
        /// </summary>
        private void OnEnable()
        {
            if (RoundManager.Instance == null || RoundManager.Instance.dungeonGenerator == null)
            {
                return;
            }

            RoundManager.Instance.dungeonGenerator.Generator?.OnGenerationStatusChanged += ApplyGlobalPropModifiers;
        }

        /// <summary>
        ///     Unsubscribe from <c>Dungeon</c> generation state change event, and reapply original <c>GlobalProp</c> count range values.
        /// </summary>
        private void OnDisable()
        {
            if (RoundManager.Instance == null || RoundManager.Instance.dungeonGenerator == null)
            {
                return;
            }

            DungeonGenerator? generator = RoundManager.Instance.dungeonGenerator.Generator;
            generator?.OnGenerationStatusChanged -= ApplyGlobalPropModifiers;

            if (generator == null || generator.DungeonFlow == null || generator.DungeonFlow.GlobalProps == null)
            {
                return;
            }

            // Check if any global prop values were modified.
            if (OriginalValues.Count == 0)
            {
                return;
            }

            // Restore previously saved global prop values.
            foreach (DungeonFlow.GlobalPropSettings globalProp in generator.DungeonFlow.GlobalProps)
            {
                if (OriginalValues.TryGetValue(globalProp.ID, out IntRange originalCount))
                {
                    globalProp.Count = originalCount;
                }
            }

            // Clear saved global prop values.
            OriginalValues.Clear();
        }

        /// <summary>
        ///     Apply all <c>GlobalProp</c> modifiers before the post-processing step.
        /// </summary>
        /// <param name="generator"><c>DungeonGenerator</c> currently generating the <c>Dungeon</c>.</param>
        /// <param name="status">Current <c>Dungeon</c> generation step.</param>
        private void ApplyGlobalPropModifiers(DungeonGenerator generator, GenerationStatus status)
        {
            if (status is not GenerationStatus.PostProcessing) // Global prop selection occurs during the post-processing step.
            {
                return;
            }

            if (generator == null || generator.DungeonFlow == null || generator.DungeonFlow.GlobalProps == null)
            {
                return;
            }

            for (int i = 0; i < globalPropModifiers?.Length; i++)
            {
                GlobalPropModifier globalPropModifier = globalPropModifiers[i];

                if (globalPropModifier.GlobalPropID == 1231) // Fire Exit count is handled by the moon itself.
                {
                    Plugin.Logger.LogWarning("Tried to modify Fire Exit (ID 1231) global prop count using GlobalPropScaler! Skipping...");

                    continue;
                }

                // Find specified ID in the global prop list for the current Dungeon.
                DungeonFlow.GlobalPropSettings? foundProp = generator.DungeonFlow.GlobalProps.Find(prop => prop.ID == globalPropModifier.GlobalPropID);

                // Check if global prop was found.
                if (foundProp == null)
                {
                    continue;
                }

                // Create separate count range and save original one, if not already done.
                if (OriginalValues.TryAdd(foundProp.ID, foundProp.Count))
                {
                    foundProp.Count = new(foundProp.Count.Min, foundProp.Count.Max);
                }

                // Apply modifier values using the current size multiplier.
                globalPropModifier.ApplyModifier(foundProp.Count, generator.LengthMultiplier);

                Plugin.Logger.LogDebug($"Modified GlobalProp with ID '{foundProp.ID}': ({OriginalValues[foundProp.ID]}) -> ({foundProp.Count})");
            }
        }
    }
}