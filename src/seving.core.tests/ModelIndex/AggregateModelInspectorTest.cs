using Microsoft.VisualStudio.TestTools.UnitTesting;
using seving.core.ModelIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.tests.ModelIndex
{
    [TestClass()]
    public class AggregateModelInspectorTest
    {
        [TestMethod]
        public void GetPropertiesTest()
        {
            AggregateModelIndexInspector inspector = new AggregateModelIndexInspector();
            var result=inspector.GetIndexInfoFromType<ModelFake>();
            Assert.AreEqual(3, result.Count());
            var idInfo = result.FirstOrDefault(x => x?.Property?.Name == "Id");
            Assert.IsNotNull(idInfo);
            Assert.AreEqual(true, idInfo.Constrain);

            var address1Info= result.FirstOrDefault(x => x?.Property?.Name == "Address1");
            Assert.IsNotNull(address1Info);
            Assert.AreEqual(false, idInfo.Constrain);
            Assert.AreEqual("adress", address1Info.ComposedKeyGroup);
            Assert.AreEqual(1, address1Info.ComposedOrder);
        }

        [TestMethod]
        public void GetValuesTest()
        {
            AggregateModelIndexInspector inspector = new AggregateModelIndexInspector();
            ModelFake fake = new ModelFake();
            fake.Address1 = "address1value";
            fake.Address2 = "address2value";
            fake.Id = "idfake";

            var result=inspector.GetIndexValuesFromModel(fake);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(fake.Id, result.First().Value);
            Assert.AreEqual(fake.Address1+fake.Address2, result.Last().Value);
        }
    }

    public class ModelFake
    {
        [AggregateModelIndex(true)]
        public string Id { get; set; }

        [AggregateModelIndex("address", 1)]
        public string? Address1 { get; set; }

        [AggregateModelIndex("address", 2)]
        public string? Address2 { get; set; }
    }
}
