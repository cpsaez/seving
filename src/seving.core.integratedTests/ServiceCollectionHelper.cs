using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using seving.core.Persistence.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.integratedTests
{
    public static class ServiceCollectionHelper
    {
        public static ServiceCollection GetServiceBuilder()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("testSettings.json", false, true)
                .Build();

            ServiceCollection result = new ServiceCollection();
            result.AddOptions();
            result.AddSingleton<IConfigurationRoot>(config);
            result.Configure<SqlServerProviderConfig>(x => config.GetSection("SqlServerProvider").Bind(x));
            result.RegisterSeving();
            return result;
        }
    }
}
