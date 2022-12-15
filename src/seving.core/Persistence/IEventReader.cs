
namespace seving.core.Persistence
{
    public interface IEventReader
    {
        Task<StreamEvent?> ReadLastEvent(Guid streamRootUid, IPersistenceProvider persistence);
    }
}