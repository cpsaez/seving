namespace seving.core.Locks
{
    /// <summary>
    ///
    /// </summary>
    public interface ILocker
    {
        /// <summary>
        /// Releases the key.
        /// </summary>
        /// <param name="key">The key.</param>
        Task ReleaseKey(string key);

        /// <summary>
        /// Tries to lock the key to prevent nobody can use it.
        /// </summary>
        /// <param name="key">The item.</param>
        /// <param name="maxExecutingInSeconds">The maximum executing in seconds. If the locker is taken more time than this parameter, it is considered released</param>
        /// <returns></returns>
        Task<bool> TryLockKey(string key, int maxExecutingInSeconds);
    }
}