using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    public class ModelIndexValue
    {
        public ModelIndexValue()
        {
            this.PropertyName=String.Empty;
            this.Value= string.Empty;
        }
        public string PropertyName { get; set; }
        public string Value { get; set; }
        public bool Constrain { get; set; }
    }
}
