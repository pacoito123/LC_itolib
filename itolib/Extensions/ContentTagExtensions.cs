using LethalLevelLoader;

namespace itolib.Extensions
{
    /// <summary>
    ///     TODO.
    /// </summary>
    public static class ContentTagExtensions
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="tagA"></param>
        /// <param name="tagB"></param>
        /// <returns></returns>
        public static bool CompareTag(this ContentTag tagA, ContentTag tagB)
        {
            return tagA.contentTagName.CompareOrdinal(tagB.contentTagName);
        }
    }
}