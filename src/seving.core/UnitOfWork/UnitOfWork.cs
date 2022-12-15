using Lendsum.Crosscutting.Common.Extensions;
using seving.core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.UnitOfWork
{
    public class UnitOfWork : IDisposable
    {
        private readonly IStreamRootFactory factory;
        private readonly IPersistenceProvider persistenceProvider;
        private IPersistenceProvider? transaction = null;
        private Dictionary<Guid, StreamRoot> streamRoots = new Dictionary<Guid, StreamRoot>();

        public UnitOfWork(IStreamRootFactory factory, IPersistenceProvider persistenceProvider)
        {
            this.factory=factory??throw new ArgumentNullException(nameof(factory));
            this.persistenceProvider=persistenceProvider??throw new ArgumentNullException(nameof(persistenceProvider));
        }

        public StreamRoot GetStreamRoot(Guid uid)
        {
           var result= streamRoots.GetOrAdd(uid, ()=> factory.Build(uid));
            return result; 
        }

        public async Task BeginTransaction()
        {
            this.transaction = await persistenceProvider.BeginScope();
            foreach (var streamRoot in streamRoots.Values)
            {
                streamRoot.SetTransaction(this.transaction);
            }
        }

        public async Task SaveAll()
        {
            foreach (var streamRoot in streamRoots.Values)
            {
                await streamRoot.Save();
            }
        }

        public async Task Commit()
        {
            if (this.transaction == null) throw new SevingException("The transaction is not initialized with BeginTransaction");

            try
            {
                await SaveAll();
                await this.transaction.Commit();
            }
            finally
            {
                foreach (var streamRoot in streamRoots.Values)
                {
                    streamRoot.ResetTransaction();
                }
                this.transaction?.Dispose();
                this.transaction = null;
            }
        }

        public void Dispose()
        {
            this.transaction?.Dispose();
        }


    }
}
