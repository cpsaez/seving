using Microsoft.VisualStudio.TestTools.UnitTesting;
using seving.core.UnitOfWork;
using seving.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Utils.Tests
{
    [TestClass()]
    public class ApplyerTests
    {
        [TestMethod()]
        public async Task ApplyTest()
        {
            var target = new ApplyerTestTarget();
            await Applyer.Apply(new Event1(), null, target);
            Assert.IsTrue(target.ConsumedEvent1);
            Assert.IsFalse(target.ConsumedEvent2);
            await Applyer.Apply(new Event2(), null, target);
            Assert.IsTrue(target.ConsumedEvent2);

            target = new ApplyerTestTarget();
            // here you can check manually the cache is in use debuggin line by line.
            await Applyer.Apply(new Event2(), null, target);
            Assert.IsTrue(target.ConsumedEvent2);
            Assert.IsFalse(target.ConsumedEvent1);
        }
    }

    public class ApplyerTestTarget : IApplyEvent<Event1>, IApplyEvent<Event2>
    {
        public bool ConsumedEvent1 { get; set; } = false;
        public bool ConsumedEvent2 { get; set; } = false;
        public async Task ApplyEvent(Event1 @event, StreamRoot streamRoot)
        {
            await Task.Run(() => { ConsumedEvent1 = true; });
        }

        public async Task ApplyEvent(Event2 @event, StreamRoot streamRoot)
        {
            await Task.Run(() => { ConsumedEvent2 = true; });
        }
    }

    public class Event1 : StreamEvent { }

    public class Event2 : StreamEvent { }
    public class Event3 : StreamEvent { }

}