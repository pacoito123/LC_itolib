using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace itolib.Extensions
{
    /// <summary>
    ///     Extensions for the <c>string</c> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///     <c>Regex</c> pattern for skipping to the first letter in a given string.
        /// </summary>
        /// <example>
        ///     ("823 Bozoros") -> ("Bozoros").
        /// </example>
        public static readonly Regex skipToLetterRegex = new(@"^[^\p{L}]+", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        ///     Skip to the first letter of a given string.
        /// </summary>
        /// <param name="value">Value to be trimmed.</param>
        /// <returns>The given string, trimmed up until the first letter.</returns>
        public static string SkipToLetters(this string value)
        {
            return skipToLetterRegex.Replace(value, string.Empty);
        }

        /// <summary>
        ///     Compare two strings using Ordinal (binary) sort rules.
        /// </summary>
        /// <remarks>Deprecated. Kept here for some backwards compatibility.</remarks>
        /// <param name="strA">First string to compare.</param>
        /// <param name="strB">Second string to compare.</param>
        /// <returns>Whether both strings are equal or not.</returns>
        [Obsolete("Will be removed in a future update.")]
        public static bool CompareOrdinal(this string strA, string strB)
        {
            return string.Equals(strA, strB, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Try generate and obtain a <c>Guid</c> hash from a given string.
        /// </summary>
        /// <param name="value">Value to be hashed.</param>
        /// <param name="result">Generated <c>Guid</c> hash, as an out parameter.</param>
        /// <returns>Whether the string was successfully hashed or not.</returns>
        public static bool TryComputeGUID(this string value, out Guid result)
        {
            result = Guid.Empty;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
            result = new(hash[..16]);

            return true;
        }
    }
}