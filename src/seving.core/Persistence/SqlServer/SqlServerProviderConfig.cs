using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Persistence.SqlServer
{
    public class SqlServerProviderConfig
    {
        public SqlServerProviderConfig()
        {
            this.SevingConnectionString = string.Empty;
        }

        public string SevingConnectionString { get; set; }
    }
}
