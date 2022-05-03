using seving.core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.UnitOfWork
{
    public class StreamRootFactory :IStreamRootFactory
    {
        private readonly IEventReader eventReader;
        private readonly IAggregateModelPersistence aggPersistence;
        private readonly IEnumerable<IStreamRootConsumer> consumers;

        public StreamRootFactory(IEventReader eventReader, IAggregateModelPersistence aggPersistence, IEnumerable<IStreamRootConsumer> consumers)
        {
            this.eventReader = eventReader; 
            this.aggPersistence = aggPersistence;
            this.consumers = consumers;
        }

        public StreamRoot Build(Guid streamRootUid)
        {
            StreamRoot result = new StreamRoot(eventReader, aggPersistence, consumers, streamRootUid);
            return result;
        }
    }
}
