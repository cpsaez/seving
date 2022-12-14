using Newtonsoft.Json;
using seving.core.ModelIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.TestModelDomain.Models
{
    public class TestModel : AggregateModelBase
    {
        public TestModel()
        {
        }

        [JsonProperty]
        [AggregateModelIndex]
        public string? Value1 { get; set; }

        [JsonProperty]
        [AggregateModelIndex]
        public string? Value2 { get; set; }

        [JsonProperty]
        [AggregateModelIndex(true)]
        public string? Value3 { get; set; }
    }
}
