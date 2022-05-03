using Lendsum.Crosscutting.Common.Extensions;
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
        private IEventReader eventReader;
        private IStreamRootConsumer[] consumers;
        private IAggregateModelPersistence aggPersistence;

        private bool initialized = false;
        private List<StreamEvent> newEvents = new List<StreamEvent>();
        private Dictionary<Type, Dictionary<string, AggregateModelBase?>> loadedModels;
        private Dictionary<Type, Dictionary<string, AggregateModelBase>> modifiedModels;

        public StreamRoot(IEventReader eventReader, IAggregateModelPersistence aggPersistence, IEnumerable<IStreamRootConsumer> consumers, Guid streamRootUid)
        {
            this.eventReader = eventReader ?? throw new ArgumentNullException(nameof(eventReader));
            this.consumers = consumers.OrderBy(x => x.Priority).ToArray();
            this.aggPersistence = aggPersistence ?? throw new ArgumentNullException(nameof(aggPersistence));
            this.loadedModels = new Dictionary<Type, Dictionary<string, AggregateModelBase?>>();
            this.modifiedModels = new Dictionary<Type, Dictionary<string, AggregateModelBase>>();
            this.Uid = streamRootUid;
        }

        public Guid Uid { get; private set; }
        public int Version { get; private set; }

        public async Task Handle(StreamEvent @event)
        {
            await Initialize();
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
            string key = instanceName;

            var typedModifiedModel = modifiedModels.GetOrAdd(typeof(T), () => new Dictionary<string, AggregateModelBase>());
            if (typedModifiedModel.ContainsKey(key))
            {
                var result = (T)typedModifiedModel[key];
                return await Task.FromResult(result);
            }

            var typedLoadedModel = loadedModels.GetOrAdd(typeof(T), () => new Dictionary<string, AggregateModelBase?>());
            if (typedLoadedModel.ContainsKey(key))
            {
                var alreadyLoaded = typedLoadedModel[key];
                if (alreadyLoaded != null)
                {
                    typedModifiedModel.Add(key, Cloner.Clone<T>(alreadyLoaded));
                }

                return await Task.FromResult((T)typedModifiedModel[key]);
            }

            // load from DB
            var fromDb = await aggPersistence.GetLast(new T() { StreamUid = this.Uid, InstanceName = instanceName }, this.Version);


            if (fromDb != null)
            {
                fromDb.Version = this.Version;
                typedLoadedModel.Add(key, fromDb);
                var result = Cloner.Clone<T>(fromDb);
                typedModifiedModel.Add(key, result);
                return result;
            }
            else
            {
                typedLoadedModel.Add(key, fromDb);
                return fromDb;
            }
        }

        public async Task<T> InitModel<T>(string? instanceName = null) where T : AggregateModelBase, new()
        {
            await Initialize();

            if (instanceName == null) instanceName = string.Empty;
            T result = new T();
            result.Version = this.Version;
            var typedModifiedModel = modifiedModels.GetOrAdd(typeof(T), () => new Dictionary<string, AggregateModelBase>());
            typedModifiedModel.AddOrReplace(instanceName, result);
            return result;
        }

        public async Task<IPersistenceProvider> Save(IPersistenceProvider persistenceProvider)
        {
            List<Task<PersistenceResultEnum>> tasks = new List<Task<PersistenceResultEnum>>();
            foreach (var @event in this.newEvents)
            {
                tasks.Add(persistenceProvider.Insert(@event));
            }

            foreach (var type in this.modifiedModels.Keys)
            {
                foreach (var key in this.modifiedModels[type].Keys)
                {
                    var modifiedModel = this.modifiedModels[type][key];
                    modifiedModel.Version = this.Version;
                    // compare if the model is a modification of the original to save it.
                    bool save = false;
                    if (this.loadedModels.ContainsKey(type) && this.loadedModels[type].ContainsKey(key))
                    {
                        var original = this.loadedModels[type][key];
                        if (original != null)
                        {
                            original.Version = this.Version;
                            if (Cloner.AreEquals(original, modifiedModel))
                            {
                                save = true;
                            }
                        }
                    }
                    else
                    {
                        save = true;
                    }

                    if (save)
                    {
                        tasks.Add(persistenceProvider.Insert(modifiedModel));
                    }
                }
            }

            if (tasks.Count == 0)
            {
                return persistenceProvider;
            }

            var result = await Task.WhenAll(tasks);
            if (result.Any(x => x != PersistenceResultEnum.Success))
            {
                if (result.Any(x => x == PersistenceResultEnum.KeyAlreadyExist))
                {
                    throw new ConcurrencyException("Error saving a persistable item because his key already exist in commit")
                        .AddStreamRootUid(this.Uid)
                        .AddStreamRootVersion(this.Version);
                }

                throw new SevingException("Some of the inserts cannot be procuded")
                        .AddStreamRootUid(this.Uid)
                        .AddStreamRootVersion(this.Version); ;
            }

            return persistenceProvider;
        }

        private async Task Initialize()
        {
            if (!initialized)
            {
                var lastEvent = await eventReader.ReadLastEvent(this.Uid);
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
