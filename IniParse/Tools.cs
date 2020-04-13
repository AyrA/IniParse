using System;

namespace IniParse
{
    /// <summary>
    /// Helper functions for the INI parser
    /// </summary>
    internal class Tools
    {
        /// <summary>
        /// Characters that are not allowed in the section header strings
        /// </summary>
        public static readonly char[] ForbiddenHeaderChar = new char[] { '\r', '\n' };

        /// <summary>
        /// Characters that are not allowed in the setting names
        /// </summary>
        public static readonly char[] ForbiddenNameChar = new char[] { '=', '\r', '\n' };

        /// <summary>
        /// Characters that are not allowed in the setting values
        /// </summary>
        public static readonly char[] ForbiddenValueChar = new char[] { '\r', '\n' };

        /// <summary>
        /// Checks if a given array is null or is empty
        /// </summary>
        /// <param name="a">Array</param>
        /// <returns><paramref name="a"/>==null || <paramref name="a"/>.Length==0</returns>
        public static bool IsEmpty(Array a)
        {
            return a == null || a.Length == 0;
        }
    }
}
