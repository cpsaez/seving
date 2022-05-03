using seving.core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Persistence
{
    public class AggregateModelPersistence : IAggregateModelPersistence
    {
        private IPersistenceProvider provider;

        public AggregateModelPersistence(IPersistenceProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }
        public async Task<T?> GetLast<T>(T prototype, int maxVersion) where T : AggregateModelBase
        {
            prototype.Version = 0;
            BatchQuery<T> query = new BatchQuery<T>()
            {
                Ascendent = false,
                Limit = 1,
                IncludeKeys = true,
                Partition = prototype.Partition,
                StartKey = prototype.Keys.Key,
            };
            prototype.Version = maxVersion;
            query.EndKey = prototype.Keys.Key;

            var result = await provider.GetByKeyPattern(query);
            return result.Items.FirstOrDefault();
        }

        public async Task Save<T>(T item) where T : AggregateModelBase
        {
            await this.provider.Insert(item);
        }
    }
}
