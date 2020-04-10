using System;
using System.Collections.Generic;
using System.Text;

namespace IniParse
{
    /// <summary>
    /// Helper functions for the INI parser
    /// </summary>
    internal class Tools
    {
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
