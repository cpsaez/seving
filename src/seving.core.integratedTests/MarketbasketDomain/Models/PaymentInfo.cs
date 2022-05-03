using System;

namespace seving.core.integratedTests.MarketbasketDomain.Models
{
    public class PaymentInfo
    {
        public Guid? PaymentMethodId { get; set; }
        public string? MoreInfo { get; set; }
    }
}
