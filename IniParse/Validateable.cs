namespace IniParse
{
    /// <summary>
    /// Provides simple data validation
    /// </summary>
    public abstract class Validateable
    {
        /// <summary>
        /// Validates the current instance and throws a <see cref="ValidationException"/> if necessary
        /// </summary>
        public abstract void Validate();
    }
}