using System;

namespace IniParse
{
    /// <summary>
    /// Whitespace parsing mode
    /// </summary>
    [Flags]
    public enum WhitespaceMode : int
    {
        /// <summary>
        /// Do not strip whitespace
        /// </summary>
        AsIs = 0,
        /// <summary>
        /// Trim section names
        /// </summary>
        TrimSections = 1,
        /// <summary>
        /// Trim names of settings
        /// </summary>
        TrimNames = TrimSections << 1,
        /// <summary>
        /// Trim values of settings
        /// </summary>
        TrimValues = TrimNames << 1
    }
}