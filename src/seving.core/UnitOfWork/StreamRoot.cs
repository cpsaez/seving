using Lendsum.Crosscutting.Common.Extensions;
using seving.core.ModelIndex;
using seving.core.Persistence;
using seving.core.Utils;
using seving.core.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.UnitOfWork
{
    public class StreamRoot
    {
        private AggregateModelIndexInspector aggIndexInspector;
        private IEventReader eventReader;
        private IIndexPersistenceProvider indexPersistenceProvider;
        private IPersistenceProvider persistenceProvider;
        private IStreamRootConsumer[] consumers;
        private bool initialized = false;
        private List<StreamEvent> newEvents = new List<StreamEvent>();
        private FromToTypes models;
        private IPersistenceProvider? transaction;

        public StreamRoot(
            IEventReader eventReader,
            IIndexPersistenceProvider indexPersistenceProvider,
            IPersistenceProvider persistenceProvider,
            IEnumerable<IStreamRootConsumer> consumers, 
            Guid streamRootUid)
        {
            this.aggIndexInspector = new AggregateModelIndexInspector();
            this.eventReader = eventReader ?? throw new ArgumentNullException(nameof(eventReader));
            this.indexPersistenceProvider = indexPersistenceProvider;
            this.persistenceProvider=persistenceProvider??throw new ArgumentNullException(nameof(persistenceProvider));
            this.consumers = consumers.OrderBy(x => x.Priority).ToArray();
            this.models = new FromToTypes();
            this.Uid = streamRootUid;
        }

        public Guid Uid { get; private set; }
        public int Version { get; private set; }

        public void SetTransaction(IPersistenceProvider provider)
        {
            this.transaction = provider;
        }

        public void ResetTransaction()
        {
            this.transaction = null;
        }

        private IPersistenceProvider persistence => this.transaction ?? this.persistenceProvider;

        public async Task Handle(StreamEvent @event)
        {
            await Initialize();
            if (@event.StreamUid != this.Uid) { throw new SevingException("The streamroot can only process events from the same streamrootuid"); }

            @event.Version = this.Version + 1;
            foreach (var consumer in consumers)
            {
                // Maybe we can paralelize this if hte priority is the same
                await consumer.Consume(@event, this);
            }

            this.Version = @event.Version;
            this.newEvents.Add(@event);
        }

        public async Task<T?> GetModel<T>(string? instanceName = null) where T : AggregateModelBase, new()
        {
            await Initialize();

            if (instanceName == null) instanceName = string.Empty;

            var target = models.GetByType<T>().GetByInstanceName(instanceName);
            if (target.ToSet == true) return target.To as T;

            // check if the value has been loaded from db
            if (target.FromSet == true)
            {
                if (target.From != null)
                {
                    models.SetTo<T>(Cloner.Clone<T>(target.From), instanceName);
                }

                return target.To as T;
            }

            // we need to upload first from db

            // load from DB
            var template = new T();
            template.InstanceName= instanceName;
            template.StreamUid= this.Uid;
            T? fromDb = await this.persistence.GetValue<T>(template);


            if (fromDb != null)
            {
                fromDb.Version = this.Version;
                models.SetFrom<T>(fromDb, instanceName);
                models.SetTo<T>(Cloner.Clone<T>(fromDb), instanceName);
            }
            else
            {
                models.SetFrom<T>(null, instanceName);
                models.SetTo<T>(null, instanceName);
            }

            return target.To as T;
        }

        public async Task<T> InitModel<T>(string? instanceName = null) where T : AggregateModelBase, new()
        {
            await Initialize();

            if (instanceName == null) instanceName = string.Empty;

            var targets = models.GetByType<T>().GetByInstanceName(instanceName);
            if ((targets.ToSet && targets.To != null) || (targets.FromSet && targets.From != null))
                throw new ConcurrencyException(S.Invariant($"The model {typeof(T).Name}.{instanceName} is already initializedd"));

            T result = new T();
            result.Version = this.Version;
            result.StreamUid = this.Uid;
            result.InstanceName= instanceName;
            models.SetTo<T>(result, instanceName);
            return result;
        }

        public async Task DestroyModel<T>(string? instanceName = null) where T : AggregateModelBase, new()
        {
            if (instanceName == null) instanceName = string.Empty;
            await Initialize();

            // force the load of the model, we need it to know what key we have to remove.
            await this.GetModel<T>(instanceName);

            models.SetTo<T>(null, instanceName);
        }

        public async Task Save()
        {
            // save events
            foreach (var @event in this.newEvents)
            {
                var result = await persistence.Insert(@event);
                CheckResult(result, S.Invariant($"error saving  event {@event.Keys.Key}"));
            }

            // save models
            foreach (var modelType in this.models.GetAll())
            {
                foreach (var instanceName in this.models.GetByType(modelType).GetAll())
                {
                    var target = this.models.GetByType(modelType).GetByInstanceName(instanceName);

                    if (target.From != null && target.To != null)
                    {
                        target.To.Version = this.Version;
                        target.From.Version = this.Version;
                        if (!Cloner.AreEquals(target.From, target.To))
                        {
                            var result = await persistence.Update(target.To);
                            CheckResult(result, S.Invariant($"error saving {target.To.Keys.Key}"));
                        }
                    }
                    else if (target.From == null && target.To != null)
                    {
                        var result = await persistence.Insert(target.To);
                        CheckResult(result, S.Invariant($"error saving {target.To.Keys.Key}"));
                    }
                    else if (target.From != null && target.To == null)
                    {
                        var result = await persistence.Delete(target.From);
                        CheckResult(result, S.Invariant($"error deleting model {target.From.Keys.Key}"));
                    }

                    // update now the indexes
                    var indexChanges = aggIndexInspector.GetChanges(target.From, target.To);
                    await this.indexPersistenceProvider.Persist(modelType, this.Uid, instanceName, indexChanges, persistence);
                }
            }
        }

        private static void CheckResult(PersistenceResultEnum result, string? errorMessage)
        {
            if (result == PersistenceResultEnum.Success) return;
            if (result == PersistenceResultEnum.KeyAlreadyExist) throw new ConcurrencyException(errorMessage);
            throw new SevingException(errorMessage ?? "Error saving something");
        }

        private async Task Initialize()
        {
            if (!initialized)
            {
                var lastEvent = await eventReader.ReadLastEvent(this.Uid, this.persistence);
                if (lastEvent != null)
                {
                    this.Version = lastEvent.Version;
                }
                else
                {
                    this.Version = 0;
                }

                initialized = true;
            }
        }
    }
}
