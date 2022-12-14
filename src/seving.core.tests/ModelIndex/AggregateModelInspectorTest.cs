using Microsoft.VisualStudio.TestTools.UnitTesting;
using seving.core.ModelIndex;
using seving.core.Utils;
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
        private AggregateModelIndexInspector inspector;

        public AggregateModelInspectorTest()
        {
            this.inspector = new AggregateModelIndexInspector();
        }

        [TestMethod]
        public void GetPropertiesTest()
        {
            var result=inspector.GetIndexInfoFromType(typeof(ModelFake));
            Assert.AreEqual(3, result.Count());
            var idInfo = result.FirstOrDefault(x => x?.Property?.Name == "Id");
            Assert.IsNotNull(idInfo);
            Assert.AreEqual(true, idInfo.Constrain);

            var address1Info= result.FirstOrDefault(x => x?.Property?.Name == "Address1");
            Assert.IsNotNull(address1Info);
            Assert.AreEqual(false, address1Info.Constrain);
        }

        [TestMethod]
        public void GetValuesTest()
        {
            ModelFake fake = new ModelFake();
            fake.Address1 = "address1value";
            fake.Address2 = "address2value";
            fake.Id = "idfake";

            var result=inspector.GetIndexValuesFromModel(fake);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(fake.Id, result.First().Value);
            Assert.AreEqual(fake.Address2, result.Last().Value);
        }
        
        [TestMethod]
        public void GetChangesNullFromValues()
        {
            ModelFake fake1 = new ModelFake();
            fake1.Address1 = "address1value";
            fake1.Address2 = "address2value";
            var result=inspector.GetChanges(fake1, null);
            Assert.IsTrue(result.All(x => x.Operation == ModelIndexOperationEnum.Delete));
        }

        [TestMethod]
        public void GetChangesNullToValues()
        {
            ModelFake fake1 = new ModelFake();
            fake1.Address1 = "address1value";
            fake1.Address2 = "address2value";
            var result = inspector.GetChanges(null, fake1);
            Assert.IsTrue(result.All(x => x.Operation == ModelIndexOperationEnum.Insert));
        }

        [TestMethod]
        public void GetChangesNoChangesTests()
        {
            AggregateModelIndexInspector inspector = new AggregateModelIndexInspector();
            ModelFake fake1 = new ModelFake();
            fake1.Address1 = "address1value";
            fake1.Address2 = "address2value";
            fake1.Id = "idfake";
            ModelFake fake2 = Cloner.Clone<ModelFake>(fake1);
            var result=inspector.GetChanges(fake1, fake2);
            Assert.AreEqual(result.Count(), 0);
        }

        [TestMethod]
        public void GetChangesTests()
        {
            AggregateModelIndexInspector inspector = new AggregateModelIndexInspector();
            ModelFake fake1 = new ModelFake();
            fake1.Address1 = "address1value";
            fake1.Address2 = "address2value";
            fake1.Id = "idfake";
            ModelFake fake2 = new ModelFake();
            fake2.Address1 = "addres2Vaulue";
            fake2.Address2 = "address2Value";
            fake2.Id = "idFake2";
            var result = inspector.GetChanges(fake1, fake2);
            Assert.AreEqual(result.Count(), 3);
            Assert.IsTrue(result.All(x => x.Operation == ModelIndexOperationEnum.UpdateOrInsert));

            // check the item in the result is ok
            var firstResult = result.First();
            Assert.AreEqual(firstResult.Old?.Value, fake1.Id);
            Assert.AreEqual(firstResult.New?.Value, fake2.Id);
        }

        [TestMethod]
        public void GetChangesToEmptyTests()
        {
            AggregateModelIndexInspector inspector = new AggregateModelIndexInspector();
            ModelFake fake1 = new ModelFake();
            fake1.Address1 = "address1value";
            fake1.Address2 = "address2value";
            fake1.Id = "idfake";
            ModelFake fake2 = new ModelFake();
            fake2.Address1 = "addres2Vaulue";
            fake2.Address2 = "address2Value";
            fake2.Id = ""; // NO id here, to this index must be removed.
            var result = inspector.GetChanges(fake1, fake2);
            Assert.AreEqual(result.Count(), 3);
            Assert.AreEqual(result.First().Operation, ModelIndexOperationEnum.Delete);
        }
    }

    public class ModelFake
    {
        public ModelFake()
        {
            this.Id = String.Empty;
        }

        [AggregateModelIndex(true)]
        public string Id { get; set; }

        [AggregateModelIndex()]
        public string? Address1 { get; set; }

        [AggregateModelIndex()]
        public string? Address2 { get; set; }
    }
}
