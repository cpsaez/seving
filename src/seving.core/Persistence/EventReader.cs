using seving.core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Persistence
{
    public class EventReader : IEventReader
    {
        private IPersistenceProvider provider;

        public EventReader(IPersistenceProvider provider)
        {
            this.provider = provider;
        }

        public async Task<StreamEvent?> ReadLastEvent(Guid streamRootUid)
        {
            var batchQuery = GetBatchQueryForLastEvent(streamRootUid);
            var result = await this.provider.GetByKeyPattern<StreamEvent>(batchQuery);
            return result.Items.FirstOrDefault();
        }

        private static StreamEvent BuildEvent(Guid streamRootUid, int version)
        {
            var streamEvent = new StreamEvent();
            streamEvent.StreamUid = streamRootUid;
            streamEvent.Version = version;
            return streamEvent;
        }

        public BatchQuery<StreamEvent> GetBatchQueryForLastEvent(Guid streamRootUid)
        {
            var query = new BatchQuery<StreamEvent>();
            query.Partition = new StreamEvent().Partition;
            query.ConstantSegment = new ComposedKey(streamRootUid, string.Empty).Key ?? string.Empty;
            query.StartKey = string.Empty;
            query.EndKey = string.Empty;
            query.Ascendent = false;
            query.Limit = 1;
            return query;
        }
    }
}
