using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using seving.core.ModelIndex;
using seving.core.Persistence;
using seving.core.Persistence.SqlServer;
using seving.core.UnitOfWork;
using seving.core.Utils.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core
{
    public static class RegistrationHelper
    {
        public static void RegisterSeving(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IPersistenceProvider, SqlServerProvider>();
            serviceCollection.AddSingleton<ITextSerializer, JsonTextSerializer>();
            serviceCollection.AddSingleton<IEventReader, EventReader>();
            serviceCollection.AddSingleton<IAggregateModelPersistence, AggregateModelPersistence>();
            serviceCollection.AddSingleton<IStreamRootFactory, StreamRootFactory>();
            serviceCollection.AddSingleton<IIndexPersistenceProvider, IndexPersistenceProvider>();
            serviceCollection.AddSingleton<IIndexSearch, IndexSearch>();
        }
    }
}
