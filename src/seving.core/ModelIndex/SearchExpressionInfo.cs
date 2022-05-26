using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    public class SearchExpressionInfo
    {
        public SearchExpressionInfo(Type aggregateModelType, ModelIndexInfo modelIndexInfo)
        {
            AggregateModelType = aggregateModelType;   
            ModelInfo = modelIndexInfo;
        }
        public Type AggregateModelType { get; set; }

        public ModelIndexInfo ModelInfo { get; set; }
    }
}
