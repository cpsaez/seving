using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Persistence
{

    /// <summary>
    /// Interface to be implemented by a IProvider
    /// </summary>
    public interface IPersistenceProvider : IDisposable
    {
        Task<IPersistenceProvider> BeginScope();
        Task Commit();
        Task<PersistenceResultEnum> Delete(IEnumerable<IPersistable> items);
        Task<PersistenceResultEnum> Delete(IPersistable item);
        Task<PersistenceResultEnum> Update(IPersistable item, ComposedKey? currentKey = null);
        Task<BatchQuery<T>> GetByKeyPattern<T>(BatchQuery<T> batchQuery) where T : IPersistable;
        Task<BatchQuery<T>> GetByKeyPattern<T>(IPersistable startItem, IPersistable endItem, int? limit = null, bool includeKeys = false, bool asc = true) where T : IPersistable;
        Task<bool> GetLock(string itemName, int maxExecutingInSeconds);
        Task<T?> GetValue<T>(IPersistable key) where T : class;
        Task<IEnumerable<T>> GetValues<T>(IEnumerable<IPersistable> keys) where T : class;
        Task<PersistenceResultEnum> Insert(IPersistable item);
        Task ReleaseLock(string itemName);
        Task<ulong> ReserveCounter(string key);
        Task Rollback();

    }
}
