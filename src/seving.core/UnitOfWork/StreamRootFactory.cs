using seving.core.ModelIndex;
using seving.core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.UnitOfWork
{
    public class StreamRootFactory : IStreamRootFactory
    {
        private readonly IEventReader eventReader;
        private readonly IIndexPersistenceProvider indexPersistenceProvider;
        private readonly IEnumerable<IStreamRootConsumer> consumers;
        private readonly IPersistenceProvider persistenceProvider;

        public StreamRootFactory(
            IEventReader eventReader,
            IPersistenceProvider persistenceProvider,
            IIndexPersistenceProvider indexPersistenceProvider,
            IEnumerable<IStreamRootConsumer> consumers)
        {

            this.eventReader = eventReader;
            this.indexPersistenceProvider = indexPersistenceProvider;
            this.consumers = consumers;
            this.persistenceProvider = persistenceProvider;
        }

        public StreamRoot Build(Guid streamRootUid)
        {
            StreamRoot result = new StreamRoot(
                eventReader, 
                indexPersistenceProvider,
                persistenceProvider,
                consumers, 
                streamRootUid);
            return result;
        }
    }
}
