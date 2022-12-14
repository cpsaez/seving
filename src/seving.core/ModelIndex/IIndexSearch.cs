using seving.core.Persistence;
using System.Linq.Expressions;

namespace seving.core.ModelIndex
{
    public interface IIndexSearch
    {
        Task<Guid?> GetExactly<T>(Expression<Func<T, string?>> expression, string value, IPersistenceProvider provider);
    }
}