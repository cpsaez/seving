using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.MarketbasketDomain.Events
{
    public class ItemRemoved : StreamEvent
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
