using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.MarketbasketDomain.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OpenOrder : AggregateModelBase
    {
        [JsonProperty]
        private List<ItemInfo> items;

        public OpenOrder()
        {
            this.items = new List<ItemInfo>();
        }

        public void AddItem(ItemInfo info)
        {
            var current = items.Where(x => x.Id == info.Id).FirstOrDefault();
            if (current != null)
            {
                current.Quantity=current.Quantity + info.Quantity;
            }
            else
            {
                items.Add(info);
            }
        }

        public void RemoveItem(int id, int qtt)
        {
            var current = items.Where(x => x.Id == id).FirstOrDefault();
            if (current != null)
            {
                current.Quantity = current.Quantity - qtt;
                if (current.Quantity<=0)
                {
                    items.Remove(current);
                }
            }
        }

        public IEnumerable<ItemInfo> Items => items.ToArray();
    }
}
