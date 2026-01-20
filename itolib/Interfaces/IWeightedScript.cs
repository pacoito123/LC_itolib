using System;

namespace itolib.Interfaces
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public interface IWeightedEntry
    {
        /// <summary>
        ///     Weight value for this specific entry.
        /// </summary>
        int Weight { get; set; }

        /// <summary>
        ///     Whether this specific entry can be used more than once or not.
        /// </summary>
        bool SingleUse { get; set; }
    }

    /// <summary>
    ///     Adds weighted selection capabilities to any implementing class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks></remarks>
    public interface IWeightedScript<T> where T : IWeightedEntry
    {
        /// <summary>
        ///     Cached instance of the implementing script as an <c>IWeightedScript</c>, to avoid having to cast.
        /// </summary>
        IWeightedScript<T> WeightedSelf { get; }

        /// <summary>
        ///     TODO.
        /// </summary>
        T[]? WeightedEntries { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        int[]? CumulativeWeights { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        int TotalWeight { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        bool InitializedWeights { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        void InitializeWeights()
        {
            if (InitializedWeights)
            {
                return;
            }

            if (WeightedEntries?.Length > 0)
            {
                int[] cumulativeWeights = new int[WeightedEntries.Length];

                for (int i = 0; i < WeightedEntries.Length; i++)
                {
                    TotalWeight += WeightedEntries[i].Weight;
                    cumulativeWeights[i] = TotalWeight;
                }

                CumulativeWeights = cumulativeWeights;
            }

            InitializedWeights = true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entry"></param>
        void AddWeight(T entry)
        {
            TotalWeight += entry.Weight;

            WeightedEntries = (WeightedEntries?.Length > 0) ? [.. WeightedEntries, entry] : [entry];
            CumulativeWeights = (CumulativeWeights?.Length > 0) ? [.. CumulativeWeights, TotalWeight] : [TotalWeight];
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entries"></param>
        void AddWeights(T[]? entries)
        {
            if (entries == null || entries.Length == 0)
            {
                // TODO: Log warning.
                return;
            }

            int[] cumulativeWeights = new int[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                TotalWeight += entries[i].Weight;
                cumulativeWeights[i] = TotalWeight;
            }

            WeightedEntries = (WeightedEntries?.Length > 0) ? [.. WeightedEntries, .. entries] : entries;
            CumulativeWeights = (CumulativeWeights?.Length > 0) ? [.. CumulativeWeights, .. cumulativeWeights] : cumulativeWeights;

            InitializedWeights = true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="index"></param>
        void RemoveWeight(int index)
        {
            if (CumulativeWeights == null || index < 0 || index > CumulativeWeights.Length)
            {
                return;
            }

            if (index > 0)
            {
                int difference = CumulativeWeights[index] - CumulativeWeights[index - 1];

                if (difference > 0)
                {
                    ModifyWeight(index, -difference);
                }
            }
            else if (CumulativeWeights[index] > 0)
            {
                ModifyWeight(index, -CumulativeWeights[index]);
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="weight"></param>
        void ModifyWeight(int index, int weight)
        {
            if (weight == 0 || index < 0 || CumulativeWeights == null || index >= CumulativeWeights.Length)
            {
                return;
            }

            if (CumulativeWeights[index] + weight < 0)
            {
                weight = -CumulativeWeights[index];
            }

            TotalWeight += weight;

            for (int i = index; i < CumulativeWeights.Length; i++)
            {
                CumulativeWeights[i] += weight;
            }
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="weightIndex"></param>
        /// <returns></returns>
        bool TryObtainEntry(out T entry, int weightIndex)
        {
            entry = default!;

            if (weightIndex >= 0 && weightIndex < WeightedEntries?.Length)
            {
                entry = WeightedEntries[weightIndex];

                if (entry.SingleUse)
                {
                    RemoveWeight(weightIndex);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        bool TryObtainRandomEntry(out T entry, Random? random = null)
        {
            entry = default!;

            if (!TryObtainRandomEntryIndex(out int weightIndex, random))
            {
                return false;
            }

            entry = WeightedEntries![weightIndex];

            if (entry.SingleUse)
            {
                RemoveWeight(weightIndex);
            }

            return true;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="weightIndex"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        bool TryObtainRandomEntryIndex(out int weightIndex, Random? random = null)
        {
            weightIndex = -1;

            if (CumulativeWeights == null || CumulativeWeights.Length == 0)
            {
                return false;
            }

            int randomWeight = (random != null) ? random.Next(0, TotalWeight + 1)
                : UnityEngine.Random.RandomRangeInt(0, TotalWeight + 1);

            weightIndex = Array.FindIndex(CumulativeWeights, weight => randomWeight <= weight);

            return weightIndex >= 0 && weightIndex < CumulativeWeights.Length;
        }
    }
}