namespace itolib.Interfaces
{
    /// <summary>
    ///     TODO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IScriptableConfigEntry<T>
    {
        /// <summary>
        ///     TODO.
        /// </summary>
        string Section { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        T DefaultValue { get; set; }

        /// <summary>
        ///     TODO.
        /// </summary>
        string Description { get; set; }
    }
}