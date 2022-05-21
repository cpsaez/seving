using Newtonsoft.Json;
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
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class AggregateModelBase : IPersistable
    {
        public AggregateModelBase()
        {
            this.InstanceName = string.Empty;
            this.Cas = String.Empty;
        }

        [JsonProperty]
        public string InstanceName { get; set; }
        [JsonProperty]
        public Guid StreamUid { get; set; }
        [JsonProperty]
        public int Version { get; set; }

        #region IPersistable

        public string Partition => this.GetType().Name;
        public ComposedKey Keys => new ComposedKey(StreamUid,InstanceName, Version);

        [JsonProperty]
        public string? Cas { get; set; }

        #endregion
    }
}
