using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using seving.core.integratedTests.MarketbasketDomain.Events;
using seving.core.integratedTests.MarketbasketDomain.Models;
using seving.core.Persistence;
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
        private IPersistenceProvider sqlServer;

        public StreamRootTests()
        {
            var builder = ServiceCollectionHelper.GetServiceBuilder();
            builder.AddSingleton<IStreamRootConsumer, OrderController>();
            var di = builder.BuildServiceProvider();
            this.factory = (di.GetService<IStreamRootFactory>() as StreamRootFactory) ?? throw new ArgumentException("Cannot instanciate StreamRootFactory");
            this.sqlServer = di.GetService<IPersistenceProvider>()?? throw new ArgumentNullException("cannot instanciate sql server");
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
            Assert.AreEqual(3, model.Items.Where(x => x.Id == 2).First().Quantity);

            await stream.Handle(new BasketCleared() { StreamUid = uid, });
            model = await stream.GetModel<OpenOrder>();
            Assert.IsNull(model);

            using (var scope = await this.sqlServer.BeginScope())
            {
                await stream.Save(scope);
                await scope.Commit();
            }

            // saving more events to the same stream, another running process (simulated)
            var savedStream = factory.Build(uid);
            var savedModel = await savedStream.GetModel<OpenOrder>();
            Assert.IsNull(savedModel);
            await savedStream.Handle(new ItemAdded() { StreamUid = uid, item = new ItemInfo() { Id = 1, Price = 50.5M, Quantity = 1 } });
            await savedStream.Handle(new ItemAdded() { StreamUid = uid, item = new ItemInfo() { Id = 2, Price = 34, Quantity = 4 } });

            using (var scope = await this.sqlServer.BeginScope())
            {
                await savedStream.Save(scope);
                await scope.Commit();
            }

            // recovering hte previously saved stream and check the model is in place
            savedStream = factory.Build(uid);
            savedModel = await savedStream.GetModel<OpenOrder>();
          
            Assert.IsNotNull(savedModel);
            Assert.AreEqual(2, savedModel.Items.Count());
            Assert.AreEqual(4, savedModel.Items.Where(x  => x.Id == 2).First().Quantity);
            Assert.AreEqual(uid, savedModel.StreamUid);
        }
    }
}
