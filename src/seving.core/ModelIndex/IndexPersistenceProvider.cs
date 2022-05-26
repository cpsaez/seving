using seving.core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    public class IndexPersistenceProvider : IIndexPersistenceProvider
    {
        
        //public async Task<PersistableIndexRow> GetExact(PersistableIndexRow row, IPersistenceProvider provider)
        //{
        //    provider.g
        //}

        public async Task Persist(Type modelType,Guid streamRootUid, string instanceName, ModelIndexComparisonResult comparisonResult, IPersistenceProvider persistenceProvider)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            var modelName = modelType.Name;

            if (comparisonResult.Operation == ModelIndexOperationEnum.Delete)
            {

                if (comparisonResult.Old == null) throw new SevingException($"We cannot delete an index with null value in Old. Index of model: {modelName}");
                PersistableIndexRow toDelete = new PersistableIndexRow()
                {
                    ModelIndexedName = modelName,
                    StreamRootUid = streamRootUid,
                    InstanceName = instanceName,
                    SearchableProperty = comparisonResult.Old.PropertyName,
                    SearchableValue = comparisonResult.Old.Value,
                    Constrain = comparisonResult.Old.Constrain,
                };

                var result = await persistenceProvider.Delete(toDelete);
                CheckResult(result, $"Error deleting {toDelete.Keys.Key}");
                return;
            }
            else if (comparisonResult.Operation == ModelIndexOperationEnum.Insert)
            {
                if (comparisonResult.New == null) throw new SevingException($"We cannot insert an index with null value in New. Index of model: {modelName}");

                PersistableIndexRow toInsert = new PersistableIndexRow()
                {
                    ModelIndexedName = modelName,
                    StreamRootUid = streamRootUid,
                    InstanceName = instanceName,
                    SearchableProperty = comparisonResult.New.PropertyName,
                    SearchableValue = comparisonResult.New.Value,
                    Constrain = comparisonResult.New.Constrain
                };

                var result = await persistenceProvider.Insert(toInsert);
                CheckResult(result, $"Error inserting {toInsert.Keys.Key}");
                return;
            }
            else if (comparisonResult.Operation == ModelIndexOperationEnum.UpdateOrInsert)
            {
                if (comparisonResult.New == null) throw new SevingException($"We cannot update an index with null value in New. Index of model: {modelName}");
                if (comparisonResult.Old == null) throw new SevingException($"We cannot update an index with null value in Old. Index of model: {modelName}");

                PersistableIndexRow old = new PersistableIndexRow()
                {
                    ModelIndexedName = modelName,
                    StreamRootUid = streamRootUid,
                    InstanceName = instanceName,
                    SearchableProperty = comparisonResult.Old.PropertyName,
                    SearchableValue = comparisonResult.Old.Value,
                    Constrain = comparisonResult.Old.Constrain
                };


                PersistableIndexRow toUpdate = new PersistableIndexRow()
                {
                    ModelIndexedName = modelName,
                    StreamRootUid = streamRootUid,
                    InstanceName = instanceName,
                    SearchableProperty = comparisonResult.New.PropertyName,
                    SearchableValue = comparisonResult.New.Value,
                    Constrain = comparisonResult.New.Constrain
                };

                var result = await persistenceProvider.Update(toUpdate, old.Keys);
                CheckResult(result, $"Error updating {toUpdate.Keys.Key} from currentKey {old.Keys.Key}");
                return;
            }
        }

        public async Task Persist(Type modelType, Guid streamRootUid, string instanceName, IEnumerable<ModelIndexComparisonResult> comparisonResults, IPersistenceProvider persistenceProvider)
        {
            foreach (var comparisonResult in comparisonResults)
            {
                await Persist(modelType, streamRootUid, instanceName, comparisonResult, persistenceProvider);
            }
        }

        private static void CheckResult(PersistenceResultEnum result, string? errorMessage)
        {
            if (result == PersistenceResultEnum.Success) return;
            if (result == PersistenceResultEnum.KeyAlreadyExist) throw new ConcurrencyException(errorMessage);
            throw new SevingException(errorMessage ?? "Error saving something");
        }
    }
}
