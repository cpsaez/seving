using Newtonsoft.Json;
using seving.core.ModelIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.MarketbasketDomain.Models
{
    public class TenderedOrder : AggregateModelBase
    {
        public TenderedOrder()
        {
            this.PaymentInfo = new PaymentInfo();
            this.items = Array.Empty<ItemInfo>().ToList();
        }

        [JsonProperty]
        [AggregateModelIndex]
        public Guid OrderUid { get; set; }

        [JsonProperty]
        private List<ItemInfo> items;

        public void SetItems(IEnumerable<ItemInfo> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (!items.Any()) throw new MarketbasketException("The items in an tendered order must at least have one item");
            this.items = items.ToList();
        }

        [JsonProperty]
        public PaymentInfo PaymentInfo { get; set; }

        [AggregateModelIndex(true)]
        public string PaymentExternalId => this.PaymentInfo.PaymentExternalId;
    }
}
