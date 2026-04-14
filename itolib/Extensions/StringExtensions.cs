using System;
using System.Security.Cryptography;
using System.Text;

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
            return string.Equals(strA, strB, StringComparison.Ordinal);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        ///     TODO.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryComputeGUID(this string value, out Guid result)
        {
            result = Guid.Empty;

            if (value.IsNullOrEmpty())
            {
                return false;
            }

            byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
            result = new Guid(hash[..16]);

            return true;
        }
    }
}