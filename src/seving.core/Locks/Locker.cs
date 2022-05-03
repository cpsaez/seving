

using seving.core.Persistence;

namespace seving.core.Locks
{
    /// <summary>
    /// Class to lock an aggregate and prevents that no more events been dispached asyncronly
    /// </summary>
    public class Locker : ILocker
    {
        private IPersistenceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Locker" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public Locker(IPersistenceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Tries to lock the key to prevent nobody can use it.
        /// </summary>
        /// <param name="key">The item.</param>
        /// <param name="maxExecutingInSeconds">The maximum executing in seconds. If the locker is taken more time than this parameter, it is considered released</param>
        /// <returns></returns>
        public async Task<bool> TryLockKey(string key, int maxExecutingInSeconds)
        {
            var result= await provider.GetLock(key, maxExecutingInSeconds);
            return result;
        }

        /// <summary>
        /// Releases the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public async Task ReleaseKey(string key)
        {
            await provider.ReleaseLock(key);
        }
    }
}