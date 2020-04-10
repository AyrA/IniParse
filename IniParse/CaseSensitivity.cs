using System;

namespace IniParse
{
    /// <summary>
    /// Handling of upper/lowercase in INI file content
    /// </summary>
    [Flags]
    public enum CaseSensitivity : int
    {
        /// <summary>
        /// Do not convert character casing
        /// </summary>
        AsIs = 0,

        /// <summary>
        /// Section names are treated case insensitive
        /// </summary>
        CaseInsensitiveSection = 1,
        /// <summary>
        /// Setting names are treated case insensitive
        /// </summary>
        CaseInsensitiveSetting = CaseInsensitiveSection << 1,
    }
}