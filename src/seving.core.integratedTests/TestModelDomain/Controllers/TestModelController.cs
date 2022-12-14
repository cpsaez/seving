using seving.core.integratedTests.TestModelDomain.Events;
using seving.core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using seving.core.integratedTests.TestModelDomain.Models;

namespace seving.core.integratedTests.TestModelDomain.Controllers
{
    public class TestModelController :
        StreamRootConsumerBase
        , IApplyEvent<ChangeModelEvent>
    {
        public TestModelController()
        {
        }

        public override int Priority => 0;

        public async Task ApplyEvent(ChangeModelEvent @event, StreamRoot streamRoot)
        {
            var model = await streamRoot.GetModel<TestModel>();
            if (model == null)
            {
                model=await streamRoot.InitModel<TestModel>(null);
                if (model == null) throw new SevingException("Cannot initialize model");
            }

            if (@event.Value1!=null) { model.Value1= @event.Value1; }
            if (@event.Value2 != null) { model.Value2 = @event.Value2; }
            if (@event.Value3 != null) { model.Value3 = @event.Value3; }
        }
    }
}
