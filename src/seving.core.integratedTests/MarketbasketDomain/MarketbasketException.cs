using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests.MarketbasketDomain
{
    public class MarketbasketException : Exception
    {
        public MarketbasketException(string message):base(message)
        {
        }
    }
}
