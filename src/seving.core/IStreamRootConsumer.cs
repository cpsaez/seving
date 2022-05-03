using seving.core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core
{
    /// <summary>
    /// Interface to be implemented by a class who wants to consume events
    /// </summary>
    public interface IStreamRootConsumer
    {
        /// <summary>
        /// Method to consume an event in the stream root context
        /// </summary>
        /// <param name="event"></param>
        /// <param name="streamRoot"></param>
        Task Consume(StreamEvent @event, StreamRoot streamRoot);

        /// <summary>
        /// the priority of this consumer, 0 has the highest priority.
        /// </summary>
        int Priority { get; }
    }
}
