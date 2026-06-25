using itolib.Enums;
using itolib.Structs;
using System;

namespace itolib.Interfaces
{
    /// <summary>
    ///     Represents a single entry with weights to be used for weighted selection.
    /// </summary>
    public interface IWeightedEntry
    {
        /// <summary>
        ///     Weight value for this specific entry.
        /// </summary>
        int Weight { get; set; }

        /// <summary>
        ///     Weight modifiers to apply whenever this specific entry is used.
        /// </summary>
        WeightedModifier[]? WeightedModifiers { get; set; }

        /// <summary>
        ///     Whether this specific entry can be used more than once or not.
        /// </summary>
        bool SingleUse { get; set; }
    }

    /// <summary>
    ///     Adds weighted selection capabilities to any implementing class.
    /// </summary>
    /// <typeparam name="T">A struct type that implements <c>IWeightedEntry</c>.</typeparam>
    public interface IWeightedScript<T> where T : struct, IWeightedEntry
    {
        /// <summary>
        ///     Cached instance of the implementing script as an <c>IWeightedScript</c>, to avoid having to cast.
        /// </summary>
        IWeightedScript<T> WeightedSelf { get; }

        /// <summary>
        ///     List of weighted entries of type <c><typeparamref name="T"/></c>.
        /// </summary>
        /// <remarks>Not intended to be modified outside of adding new entries.</remarks>
        T[]? WeightedEntries { get; set; }

        /// <summary>
        ///     List of actual weights for each entry index. 
        /// </summary>
        int[]? CurrentWeights { get; set; }

        /// <summary>
        ///     Total sum of all weighted entries.
        /// </summary>
        int TotalWeight { get; set; }

        /// <summary>
        ///     Whether weighted entries have been initialized or not.
        /// </summary>
        bool InitializedWeights { get; set; }

        /// <summary>
        ///     Initialize weighted entries (if not done already).
        /// </summary>
        void InitializeWeights()
        {
            if (!InitializedWeights)
            {
                AddWeights(WeightedEntries);
            }
        }

        /// <summary>
        ///     Add a single weighted entry of type <c><typeparamref name="T"/></c>.
        /// </summary>
        /// <param name="entry">Entry of type <c><typeparamref name="T"/></c> to add.</param>
        void AddWeight(T entry)
        {
            TotalWeight += entry.Weight;

            WeightedEntries = (WeightedEntries?.Length > 0) ? [.. WeightedEntries, entry] : [entry];
            CurrentWeights = (CurrentWeights?.Length > 0) ? [.. CurrentWeights, entry.Weight] : [entry.Weight];

            InitializedWeights = true;
        }

        /// <summary>
        ///     Add multiple weighted entries of type <c><typeparamref name="T"/></c>.
        /// </summary>
        /// <param name="entries">Entries of type <c><typeparamref name="T"/></c> to add.</param>
        void AddWeights(T[]? entries)
        {
            if (entries == null || entries.Length == 0)
            {
                Plugin.StaticLogger.LogWarning($"Tried to add empty or null weights array to IWeightedScript '{GetType()}'!");

                return;
            }

            int[] currentWeights = new int[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                TotalWeight += entries[i].Weight;

                currentWeights[i] = entries[i].Weight;
            }

            WeightedEntries = (WeightedEntries?.Length > 0) ? [.. WeightedEntries, .. entries] : entries;
            CurrentWeights = (CurrentWeights?.Length > 0) ? [.. CurrentWeights, .. currentWeights] : currentWeights;

            InitializedWeights = true;
        }

        /// <summary>
        ///     Remove weights for the weighted entry of type <c><typeparamref name="T"/></c> at the specified index.
        /// </summary>
        /// <remarks>Sets weights to <c>0</c> instead of actually removing them.</remarks>
        /// <param name="index">Index of the entry of type <c><typeparamref name="T"/></c> to remove.</param>
        void RemoveWeight(int index)
        {
            if (index >= 0 && index < CurrentWeights?.Length)
            {
                ModifyWeight(index, 0);
            }
        }

        /// <summary>
        ///     Modify weights for the weighted entry of type <c><typeparamref name="T"/></c> at the specified index.
        /// </summary>
        /// <param name="index">Index of the entry of type <c><typeparamref name="T"/></c> to set.</param>
        /// <param name="weight">Weight value to set for the entry of type <c><typeparamref name="T"/></c> at the specified index.</param>
        void ModifyWeight(int index, int weight)
        {
            if (index >= 0 && index < CurrentWeights?.Length)
            {
                if (weight < 0) // Minimum weight for entries is zero.
                {
                    weight = 0;
                }

                int difference = weight - CurrentWeights[index];

                CurrentWeights[index] += difference;
                TotalWeight += difference;
            }
        }

        /// <summary>
        ///     Attempt to obtain a weighted entry of type <c><typeparamref name="T"/></c> at the specified index.
        /// </summary>
        /// <param name="weightEntry">Weighted entry of type <c><typeparamref name="T"/></c> at the specified index, as an out parameter.</param>
        /// <param name="weightIndex">Index of the entry of type <c><typeparamref name="T"/></c> to obtain.</param>
        /// <returns>Whether an entry of type <c><typeparamref name="T"/></c> was successfully obtained or not.</returns>
        bool TryObtainEntry(out T weightEntry, int weightIndex)
        {
            weightEntry = default;

            if (WeightedEntries == null || weightIndex < 0 || weightIndex >= WeightedEntries.Length)
            {
                Plugin.StaticLogger.LogWarning($"No weights defined for IWeightedScript '{GetType()}'!");

                return false;
            }

            weightEntry = WeightedEntries[weightIndex];

            if (weightEntry.SingleUse)
            {
                RemoveWeight(weightIndex);
            }

            for (int i = 0; i < weightEntry.WeightedModifiers?.Length; i++) // Apply weighted modifiers.
            {
                WeightedModifier modifier = weightEntry.WeightedModifiers[i];

                if (modifier.modifierIndex >= 0 && modifier.modifierIndex < CurrentWeights?.Length)
                {
                    float weight = CurrentWeights[i];

                    if (WeightedEntries[modifier.modifierIndex].SingleUse && weight == 0.0f)
                    {
                        continue;
                    }

                    weight = modifier.modifierType switch
                    {
                        ModifierType.Additive => weight + modifier.modifierValue,
                        ModifierType.Multiplicative => weight * Math.Abs(modifier.modifierValue),
                        _ => 0.0f,
                    };

                    ModifyWeight(modifier.modifierIndex, (int)weight);
                }
            }

            return true;
        }

        /// <summary>
        ///     Attempt to obtain a random weighted entry of type <c><typeparamref name="T"/></c>.
        /// </summary>
        /// <param name="weightEntry">Weighted entry of type <c><typeparamref name="T"/></c> obtained, as an out parameter.</param>
        /// <param name="weightIndex">Index of the entry of type <c><typeparamref name="T"/></c> obtained, as an out parameter.</param>
        /// <param name="random">Optional seeded <c>Random</c> instance to use.</param>
        /// <returns>Whether a random entry of type <c><typeparamref name="T"/></c> was successfully obtained or not.</returns>
        bool TryObtainRandomEntry(out T weightEntry, out int weightIndex, Random? random = null)
        {
            weightIndex = -1;
            weightEntry = default;

            if (CurrentWeights == null || CurrentWeights.Length == 0 || TotalWeight <= 0)
            {
                Plugin.StaticLogger.LogWarning($"No weights defined for IWeightedScript '{GetType()}'!");

                return false;
            }

            int randomWeight = (random != null) ? random.Next(0, TotalWeight + 1)
                : UnityEngine.Random.RandomRangeInt(0, TotalWeight + 1);

            for (int i = 0; i < CurrentWeights.Length; i++)
            {
                int weight = CurrentWeights[i];

                if (randomWeight < weight)
                {
                    weightIndex = i;

                    break;
                }

                randomWeight -= weight;
            }

            return TryObtainEntry(out weightEntry, weightIndex);
        }
    }
}