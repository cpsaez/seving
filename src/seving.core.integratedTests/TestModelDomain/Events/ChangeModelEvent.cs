using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.TestModelDomain.Events
{
    public class ChangeModelEvent : StreamEvent
    {
        public ChangeModelEvent()
        {
        }

        public string? Value1 { get; set; }
        public string? Value2 { get; set; }
        public string? Value3 { get; set; }
    }
}
