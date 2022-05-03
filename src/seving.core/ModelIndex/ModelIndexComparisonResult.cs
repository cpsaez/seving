using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    public class ModelIndexComparisonResult
    {
        public ModelIndexComparisonResult(ModelIndexValue? old, ModelIndexValue? @new, ModelIndexOperationEnum operation)
        {
            this.Old = old;
            this.New = @new;   
            this.Operation = operation;
        }

        public ModelIndexValue? Old { get; private set; }
        public ModelIndexValue? New { get; private set; }
        public ModelIndexOperationEnum Operation { get; private set; }
    }
}
