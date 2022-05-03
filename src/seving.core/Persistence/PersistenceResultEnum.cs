namespace seving.core.Persistence
{
    /// <summary>
    /// Represent the result of a persistance operation.
    /// </summary>
    public enum PersistenceResultEnum
    {
        /// <summary>
        /// The not set
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// The success
        /// </summary>
        Success = 10,

        /// <summary>
        /// The key already exist
        /// </summary>
        KeyAlreadyExist = 20,

        /// <summary>
        /// The document out of date
        /// </summary>
        DocumentOutOfDate = 30,

        /// <summary>
        /// The non defined error
        /// </summary>
        NonDefinedError = 40
    }
}