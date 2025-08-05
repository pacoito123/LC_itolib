using System;

namespace itolib.Interfaces
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISeededScript<T> where T : ISeededScript<T>
    {
        /// <summary>
        ///     Obtain (or create) a seeded Random instance using the current map seed.
        /// </summary>
        static Random SeededRandom
        {
            get
            {
                if (StartOfRound.Instance != null && StartOfRound.Instance.randomMapSeed != CurrentSeed) // TODO: Set to null upon scene unload?
                {
                    field = new(StartOfRound.Instance.randomMapSeed + SeedOffset);
                    CurrentSeed = StartOfRound.Instance.randomMapSeed;
                }

                return (field) ?? new();
            }
            private set;
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        private static int CurrentSeed { get; set; } = -1;

        /// <summary>
        ///     TODO.
        /// </summary>
        static int SeedOffset { get; set; }
    }
}