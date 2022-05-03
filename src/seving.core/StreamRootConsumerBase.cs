using seving.core.UnitOfWork;
using seving.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core
{
    /// <summary>
    /// Base class for any consumer for event inside an stream root in a sync process.
    /// </summary>
    public abstract class StreamRootConsumerBase : IStreamRootConsumer
    {
        public abstract int Priority { get; }

        public async Task Consume(StreamEvent @event, StreamRoot streamRoot)
        {
            await Applyer.Apply(@event, streamRoot, this);
        }
    }
}
