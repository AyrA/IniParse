namespace IniParse
{
    /// <summary>
    /// Specifies how invalid lines are handled
    /// </summary>
    public enum InvalidLineMode : int
    {
        /// <summary>
        /// Throw an exception. This is the default
        /// </summary>
        Throw = 0,
        /// <summary>
        /// Silently skip the line
        /// </summary>
        Skip = 1,
        /// <summary>
        /// Convert the line into a proper comment
        /// </summary>
        Convert = 2
    }
}