using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple =true)]  // multiuse attribute  
    public class AggregateModelIndexAttribute : System.Attribute
    {
        public string ComposedKeyGroup { get; } = string.Empty;
        public int ComposedOrder { get; }
        public bool Constrain { get; } = false;

        public AggregateModelIndexAttribute()
        {
        }

        public AggregateModelIndexAttribute(string composedKeyGroup, int composedOrder)
        {
            this.ComposedKeyGroup = composedKeyGroup;
            this.ComposedOrder = composedOrder;
        }

        public AggregateModelIndexAttribute(bool constrain)
        {
            this.Constrain = constrain;
        }
    }
}
