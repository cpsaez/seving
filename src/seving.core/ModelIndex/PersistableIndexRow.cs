using seving.core.Persistence;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    public class PersistableIndexRow : IPersistable
    {
        public PersistableIndexRow()
        {
            this.ModelIndexedName = string.Empty;
            this.SearchableValue = string.Empty;
            this.SearchableProperty = string.Empty;
            this.InstanceName = string.Empty;
        }

        public bool Constrain { get; set; }

        public string ModelIndexedName { get; set; }

        public string SearchableProperty { get; set; }

        public string SearchableValue { get; set; }

        public Guid StreamRootUid { get; set; }

        public string InstanceName { get; set; }

        public string Partition => this.ModelIndexedName + this.SearchableProperty + "Index";

        public ComposedKey Keys
        {
            get
            {
                if (this.Constrain)
                {
                    return new ComposedKey(this.SearchableValue);
                }
                else
                {
                    return new ComposedKey(this.SearchableValue, this.StreamRootUid);
                }
            }
        }

        public string? Cas { get; set; }
    }
}
