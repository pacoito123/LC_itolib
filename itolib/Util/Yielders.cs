using System.Collections.Generic;
using UnityEngine;

namespace itolib.Util
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public static class Yielders
    {
        private static readonly Dictionary<float, WaitForSeconds> cachedYielders = [];

        /// <summary>
        ///     TODO.
        /// </summary>
        public static WaitForEndOfFrame WaitForEndOfFrame { get; } = new();

        /// <summary>
        ///     TODO.
        /// </summary>
        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            if (!cachedYielders.TryGetValue(seconds, out WaitForSeconds yielder))
            {
                yielder = new(seconds);
                cachedYielders.Add(seconds, yielder);
            }

            return yielder;
        }
    }
}