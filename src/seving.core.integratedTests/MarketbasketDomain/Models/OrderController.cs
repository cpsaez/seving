using seving.core.integratedTests.MarketbasketDomain.Events;
using seving.core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.MarketbasketDomain.Models
{
    public class OrderController :
        StreamRootConsumerBase,
        IApplyEvent<ItemAdded>,
        IApplyEvent<ItemRemoved>,
        IApplyEvent<BasketCleared>
    {
        public override int Priority => 1000;

        public async Task ApplyEvent(ItemAdded @event, StreamRoot streamRoot)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            if (@event.item == null) throw new ArgumentNullException("event cannot have item null");

            OpenOrder openOrder = await GetModelOrCreate(streamRoot);
            openOrder.AddItem(@event.item);
        }

        public async Task ApplyEvent(ItemRemoved @event, StreamRoot streamRoot)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            OpenOrder openOrder = await GetModelOrCreate(streamRoot);
            openOrder.RemoveItem(@event.ItemId, @event.Quantity);
        }

        public async Task ApplyEvent(BasketCleared @event, StreamRoot streamRoot)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            await streamRoot.DestroyModel<OpenOrder>();
        }

        private async Task<OpenOrder> GetModelOrCreate(StreamRoot streamRoot)
        {
            var openOrder = await streamRoot.GetModel<OpenOrder>();
            if (openOrder == null)
            {
                openOrder = await streamRoot.InitModel<OpenOrder>();
            }

            return openOrder;
        }
    }
}
