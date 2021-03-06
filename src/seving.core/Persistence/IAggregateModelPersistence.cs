
namespace seving.core.Persistence
{
    public interface IAggregateModelPersistence
    {
        Task<T?> GetLast<T>(T prototype, int maxVersion) where T : AggregateModelBase;
    }
}