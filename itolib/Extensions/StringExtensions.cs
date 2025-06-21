namespace itolib.Extensions
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="strA"></param>
        /// <param name="strB"></param>
        /// <returns></returns>
        public static bool CompareOrdinal(this string strA, string strB)
        {
            return string.CompareOrdinal(strA, strB) == 0;
        }
    }
}