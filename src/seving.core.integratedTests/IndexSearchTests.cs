using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using seving.core.integratedTests.MarketbasketDomain.Events;
using seving.core.integratedTests.MarketbasketDomain.Models;
using seving.core.integratedTests.TestModelDomain.Controllers;
using seving.core.integratedTests.TestModelDomain.Events;
using seving.core.integratedTests.TestModelDomain.Models;
using seving.core.ModelIndex;
using seving.core.Persistence;
using seving.core.Persistence.SqlServer;
using seving.core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests
{
    [TestClass()]
    public class IndexSearchTest
    {
        private readonly IIndexSearch? indexSearch;
        private readonly IPersistenceProvider provider;
        private readonly StreamRootFactory factory;

        public IndexSearchTest()
        {
            var builder = ServiceCollectionHelper.GetServiceBuilder();
            builder.AddSingleton<IStreamRootConsumer, OrderController>();
            builder.AddSingleton<IStreamRootConsumer, TenderedOrderController>();
            builder.AddSingleton<IStreamRootConsumer, TestModelController>();


            var di = builder.BuildServiceProvider();
            this.indexSearch = di.GetService<IIndexSearch>()?? throw new ArgumentNullException("indexSearch");
            this.provider = di.GetService<IPersistenceProvider>() ?? throw new ArgumentNullException("persistenceProvider");
            this.factory = (di.GetService<IStreamRootFactory>() as StreamRootFactory) ?? throw new ArgumentException("Cannot instanciate StreamRootFactory");

            
        }

        [TestMethod]
        public async Task MarketBasketTest()
        {
            if (this.indexSearch == null) throw new ArgumentNullException("indexSearch");

            // search something that doesnt exists
            var target = Guid.NewGuid();
            Guid? result=await this.indexSearch.GetExactly<TenderedOrder>(x => x.PaymentExternalId, target.ToString(), provider);
            Assert.IsNull(result);

            // create the payment and search for it
            var paymentUid = Guid.NewGuid();
            await CreateScenario(target, paymentUid);
            result= await this.indexSearch.GetExactly<TenderedOrder>(x => x.PaymentExternalId, paymentUid.ToString(), provider);
            Assert.AreEqual(target, result);
        }

        [TestMethod]
        public async Task ConstrainTest()
        {
            if (this.indexSearch == null) throw new ArgumentNullException("indexSearch");

            await ((SqlServerProvider)this.provider).DeleteAll();
            Guid guid1= Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            await CreateTestScenario(guid1, null, null, "constant");

            bool concurrencyFound = false;
            try
            {
                await CreateTestScenario(guid2, null, null, "constant");
            }
            catch (ConcurrencyException)
            {
                concurrencyFound = true;
            }

            Assert.IsTrue(concurrencyFound);

            // check the original constant is guid1
            var searchResult = await this.indexSearch.GetExactly<TestModel>(x => x.Value3, "constant", this.provider);
            Assert.IsNotNull(searchResult);
            Assert.AreEqual(searchResult, guid1);

            // change the value of the constrain
            await CreateTestScenario(guid1, null, null, "constant2");
            await CreateTestScenario(guid2, null, null, "constant");

            searchResult = await this.indexSearch.GetExactly<TestModel>(x => x.Value3, "constant", this.provider);
            Assert.IsNotNull(searchResult);
            Assert.AreEqual(searchResult, guid2);

            searchResult = await this.indexSearch.GetExactly<TestModel>(x => x.Value3, "constant2", this.provider);
            Assert.IsNotNull(searchResult);
            Assert.AreEqual(searchResult, guid1);


        }


        public async Task CreateTestScenario(Guid uid, string? value1, string? value2, string? value3)
        {
            using (var trans = await this.provider.BeginScope())
            {
                var stream = factory.Build(uid);
                await stream.Handle(new ChangeModelEvent() { StreamUid = uid, Value1 = value1, Value2 = value2, Value3 = value3 });
                await stream.Save(trans);
                await trans.Commit();
            }
        }


        public async Task CreateScenario(Guid uid, Guid paymentUid)
        {
            var stream = factory.Build(uid);
            await stream.Handle(new ItemAdded() { StreamUid = uid, item = new ItemInfo() { Id = 1, Price = 50.5M, Quantity = 1 } });
            await stream.Handle(new ItemAdded() { StreamUid = uid, item = new ItemInfo() { Id = 2, Price = 34, Quantity = 4 } });
            await stream.Handle(new OrderPaid() { StreamUid = uid, PaymentInfo = new PaymentInfo() { PaymentExternalId = paymentUid.ToString() } });
            await stream.Save(this.provider);
        }
    }
}
