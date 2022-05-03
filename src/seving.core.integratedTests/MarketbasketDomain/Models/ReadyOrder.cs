using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.MarketbasketDomain.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ReadyOrder : AggregateModelBase
    {
        [JsonProperty]
        private List<ItemInfo> items;

        public ReadyOrder()
        {
            items = new List<ItemInfo>();
            PaymentInfo = new PaymentInfo();
            Address = string.Empty;
            TrackingCode= string.Empty; 
        }

        public PaymentInfo PaymentInfo { get; set; }

        public string Address { get; set; }

        public string TrackingCode { get; set;}

        public IEnumerable<ItemInfo> Items => items.ToArray();
    }
}
