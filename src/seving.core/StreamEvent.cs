using seving.core.Persistence;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core
{
    /// <summary>
    /// Base class to be inherited by a business event.
    /// </summary>
    public class StreamEvent : IPersistable
    {
        internal const int BIGGEST_VERSION=99999999;

        public StreamEvent()
        {
            this.When = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the date when this events is generated for the first time.
        /// </summary>
        public DateTime When { get; set; }

        /// <summary>
        /// Gets or sets the stream uid where this event belongs to.
        /// </summary>
        public Guid StreamUid { get; set; }

        /// <summary>
        /// Gets or sets the order of this event inside the event stream.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or set the context under this event has been created.
        /// </summary>
        public Guid ContextUid { get; set; }

        #region IPersistable

        public string Partition => "StreamEvent";

        public string? Cas { get; set; }

        public ComposedKey Keys => new ComposedKey(StreamUid, Version);

        #endregion


    }
}
