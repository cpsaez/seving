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
    public class IndexSearchTest
    {
        [TestMethod]
        public void ExpressionEvaluatorTest()
        {
            IndexSearch search= new IndexSearch();
            search.GetExactly<ModelFake>(x => x.Id, "bla");
        }
    }
}
