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
            var startKey = BuildEvent(streamRootUid, 0);
            var endKey = BuildEvent(streamRootUid, StreamEvent.BIGGEST_VERSION);

            BatchQuery<StreamEvent> query = new BatchQuery<StreamEvent>()
            {
                StartKey = startKey.Keys.Key,
                EndKey = endKey.Keys.Key,
                Partition = startKey.Partition,
                Limit = 1,
                Ascendent = false,
                IncludeKeys = true
            };

            var result = await this.provider.GetByKeyPattern<StreamEvent>(query);
            return result.Items.FirstOrDefault();
        }

        private static StreamEvent BuildEvent(Guid streamRootUid, int version)
        {
            var streamEvent = new StreamEvent();
            streamEvent.StreamUid = streamRootUid;
            streamEvent.Version = version;
            return streamEvent;
        }
    }
}
