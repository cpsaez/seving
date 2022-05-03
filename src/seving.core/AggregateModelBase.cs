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
    /// Base class to be used by a model created from an event stream.
    /// </summary>
    public abstract class AggregateModelBase : IPersistable
    {
        public AggregateModelBase()
        {
            this.InstanceName = string.Empty;
            this.Cas = String.Empty;
        }

        public string InstanceName { get; set; }
        public Guid StreamUid { get; set; }
        public int Version { get; set; }

        #region IPersistable

        public string Partition => this.GetType().Name;
        public ComposedKey Keys => new ComposedKey(StreamUid,InstanceName, Version);
        public string? Cas { get; set; }

        #endregion
    }
}
