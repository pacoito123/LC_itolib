using System;

namespace itolib.Extensions
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static float Next(this Random random, float minValue, float maxValue)
        {
            return (float)((random.NextDouble() * (maxValue - minValue)) + minValue);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static double Next(this Random random, double minValue, double maxValue)
        {
            return (random.NextDouble() * (maxValue - minValue)) + minValue;
        }
    }
}