using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using seving.core.Persistence;
using seving.core.Persistence.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests
{
    [TestClass()]
    public class SqlPersistenceTests
    {
        private SqlServerProvider persistenceProvider;

        public SqlPersistenceTests()
        {
            var builder = ServiceCollectionHelper.GetServiceBuilder();
            var di = builder.BuildServiceProvider();
            this.persistenceProvider = (di.GetService<IPersistenceProvider>() as SqlServerProvider) ?? throw new ArgumentException("Cannot instanciate persistence provider");
        }

        [TestMethod()]
        public async Task InsertGetTest()
        {
            StreamEvent item = new StreamEvent()
            {
                StreamUid = Guid.NewGuid(),
                Version = 1
            };

            await this.persistenceProvider.Insert(item);
            var item2 = await this.persistenceProvider.GetValue<StreamEvent>(item);
            Assert.IsNotNull(item2);
            Assert.AreEqual(item2.StreamUid, item2.StreamUid);
        }
    }
}
