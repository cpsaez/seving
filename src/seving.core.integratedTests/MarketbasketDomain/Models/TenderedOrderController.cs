using seving.core.integratedTests.MarketbasketDomain.Events;
using seving.core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.MarketbasketDomain.Models
{
    public class TenderedOrderController :
        StreamRootConsumerBase,
        IApplyEvent<OrderPaid>
    {
        public override int Priority => 1000;

        public async Task ApplyEvent(OrderPaid @event, StreamRoot streamRoot)
        {
            if (@event.PaymentInfo == null) throw new MarketbasketException("The order cannot be tendered without payment info");

            // get current open order
            var openOrder= await streamRoot.GetModel<OpenOrder>();
            if (openOrder == null) throw new MarketbasketException("The order has to have at least some items");

            var tenderedOrder=await streamRoot.InitModel<TenderedOrder>(@event.OrderGuid.ToString());
            tenderedOrder.PaymentInfo= @event.PaymentInfo;
            tenderedOrder.SetItems(openOrder.Items);
            await streamRoot.DestroyModel<OpenOrder>();
        }
    }
}
