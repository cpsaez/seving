using seving.core.Persistence;

namespace seving.core.ModelIndex
{
    public interface IIndexPersistenceProvider
    {
        Task Persist(Type modelType, Guid streamRootUid, string instanceName, IEnumerable<ModelIndexComparisonResult> comparisonResults, IPersistenceProvider persistenceProvider);
        Task Persist(Type modelType, Guid streamRootUid, string instanceName, ModelIndexComparisonResult comparisonResult, IPersistenceProvider persistenceProvider);
    }
}