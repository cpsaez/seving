using seving.core.integratedTests.MarketbasketDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.MarketbasketDomain.Events
{
    public class OrderPaid : StreamEvent
    {
        public PaymentInfo? PaymentInfo { get; set; }
        public Guid ReferenceCode { get; set; }
    }
}
