using System;

namespace seving.core.integratedTests.MarketbasketDomain.Models
{
    public class PaymentInfo
    {
        public PaymentInfo()
        {
            this.PaymentExternalId = String.Empty;
        }

        public Guid PaymentMethodId { get; set; }
        public string PaymentExternalId { get; set; }
    }
}
