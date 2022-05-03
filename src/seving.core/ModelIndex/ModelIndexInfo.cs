using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    public class ModelIndexInfo
    {
        public ModelIndexInfo(PropertyInfo property)
        {
            Property = property;
        }

        public PropertyInfo Property { get; set; }

        public bool Constrain { get; set; }

        public string? ComposedKeyGroup { get; set; }

        public int ComposedOrder { get; set; }
    }
}
