using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using seving.core.integratedTests.MarketbasketDomain.Events;
using seving.core.integratedTests.MarketbasketDomain.Models;
using seving.core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.UnitOfWork
{

    [TestClass()]
    public class StreamRootTests
    {
        private StreamRootFactory factory;

        public StreamRootTests()
        {
            var builder = ServiceCollectionHelper.GetServiceBuilder();
            builder.AddSingleton<IStreamRootConsumer, OrderController>();
            var di = builder.BuildServiceProvider();
            this.factory = (di.GetService<IStreamRootFactory>() as StreamRootFactory) ?? throw new ArgumentException("Cannot instanciate StreamRootFactory");
        }

        [TestMethod()]
        public async Task OrderFlowTest()
        {
            var uid = Guid.NewGuid();
            var stream=factory.Build(uid);
            await stream.Handle(new ItemAdded() { StreamUid = uid, item = new ItemInfo() { Id = 1, Price = 50.5M, Quantity = 1 } });
            await stream.Handle(new ItemAdded() { StreamUid = uid, item = new ItemInfo() { Id = 2, Price = 34, Quantity = 4 } });
            await stream.Handle(new ItemRemoved() { StreamUid = uid, ItemId = 2, Quantity = 1 });

            var model=await stream.GetModel<OpenOrder>();
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Items.Count());
            Assert.AreEqual(1, model.Items.Where(x => x.Id == 3).First().Quantity);

        }
    }
}
