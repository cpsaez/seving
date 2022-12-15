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
        public EventReader()
        {
        }

        public async Task<StreamEvent?> ReadLastEvent(Guid streamRootUid, IPersistenceProvider persistence)
        {
            var batchQuery = GetBatchQueryForLastEvent(streamRootUid);
            var result = await persistence.GetByKeyPattern<StreamEvent>(batchQuery);
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
