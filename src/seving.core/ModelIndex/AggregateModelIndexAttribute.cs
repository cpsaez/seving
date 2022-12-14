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
        public bool Constrain { get; } = false;

        public AggregateModelIndexAttribute()
        {
        }

        public AggregateModelIndexAttribute(bool constrain)
        {
            this.Constrain = constrain;
        }
    }
}
