using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.ModelIndex
{
    public class AggregateModelIndexInspector
    {
        private static readonly Dictionary<Type, IEnumerable<ModelIndexInfo>> cache = new Dictionary<Type, IEnumerable<ModelIndexInfo>>();

        public AggregateModelIndexInspector()
        { }

        public IEnumerable<ModelIndexValue> GetIndexValuesFromModel<T>(T model)
        {
            var indexesInfo = GetIndexInfoFromType<T>();
            if (indexesInfo == null) return Array.Empty<ModelIndexValue>();

            List<ModelIndexValue> result = new List<ModelIndexValue>();
            foreach (var info in indexesInfo)
            {
                if (!string.IsNullOrWhiteSpace(info.ComposedKeyGroup))
                {
                    // its a composedKey, check if is not already added
                    if (!result.Any(x => x.PropertyName == info.ComposedKeyGroup))
                    {

                        var composedInfos = indexesInfo.Where(x => x.ComposedKeyGroup == info.ComposedKeyGroup).OrderBy(x => x.ComposedOrder);
                        var item = new ModelIndexValue() { Constrain = false, PropertyName = info.ComposedKeyGroup, Value = String.Empty };
                        foreach (var composedInfo in composedInfos)
                        {
                            item.Value = item.Value + composedInfo.Property.GetValue(model)?.ToString() ?? String.Empty;


                        }

                        result.Add(item);
                    }
                }
                else
                {
                    result.Add(new ModelIndexValue()
                    {
                        Constrain = info.Constrain,
                        PropertyName = info.Property.Name,
                        Value = info.Property.GetValue(model)?.ToString() ?? String.Empty
                    });
                }
            }

            return result;
        }

        public IEnumerable<ModelIndexInfo> GetIndexInfoFromType<T>()
        {
            if (cache.ContainsKey(typeof(T))) return cache[typeof(T)];

            lock (cache)
            {
                List<ModelIndexInfo> result = new List<ModelIndexInfo>();
                IEnumerable<PropertyInfo> properties = typeof(T).GetProperties().Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(AggregateModelIndexAttribute)));
                foreach (var property in properties)
                {
                    foreach (var attribute in property.GetCustomAttributes(typeof(AggregateModelIndexAttribute), true))
                    {
                        AggregateModelIndexAttribute attributeInfo = (AggregateModelIndexAttribute)attribute;

                        result.Add(new ModelIndexInfo(property)
                        {
                            Constrain = attributeInfo.Constrain,
                            ComposedKeyGroup = attributeInfo.ComposedKeyGroup,
                            ComposedOrder = attributeInfo.ComposedOrder

                        });
                    }
                }

                if (!cache.ContainsKey(typeof(T)))
                {
                    cache.Add(typeof(T), result);
                }

                return result;
            }
        }

        public IEnumerable<ModelIndexComparisonResult> GetChanges<T>(T? from, T? to)
        {
            var result = new List<ModelIndexComparisonResult>();
            if (from == null && to == null) return result;
            if (from == null && to != null)
            {
                // all inserts
                return GetIndexValuesFromModel(to)
                    .Select(x => new ModelIndexComparisonResult(null, x, ModelIndexOperationEnum.Insert))
                    .ToArray();
            }
            else if (from != null && to == null)
            {
                // all deletes
                return GetIndexValuesFromModel(from)
                    .Select(x => new ModelIndexComparisonResult(x, null, ModelIndexOperationEnum.Delete))
                    .ToArray();
            }

            var fromInfo = GetIndexValuesFromModel(from);
            var toInfo = GetIndexValuesFromModel(to);

            // check keys to delete (they are in from and not in to)
            var toDelete = fromInfo.Where(from => !toInfo.Any(to => to.PropertyName == from.PropertyName));
            result.AddRange(toDelete.Select(x=>new ModelIndexComparisonResult(x, null, ModelIndexOperationEnum.Delete)));

            // to insert
            var toInsert = toInfo.Where(to => !fromInfo.Any(from => to.PropertyName == from.PropertyName));
            result.AddRange(toInsert.Select(x => new ModelIndexComparisonResult(null,x, ModelIndexOperationEnum.Insert)));

            // to update, check if the values are differents or not
            foreach (var item in fromInfo.Where(x=>!toDelete.Any(y=>y.PropertyName==x.PropertyName)))
            {
                var toItem = toInfo.Where(x => x.PropertyName == item.PropertyName).First();
                if (toItem.Value!=item.Value)
                {
                    ModelIndexOperationEnum operation = ModelIndexOperationEnum.UpdateOrInsert;

                    // we dont index empty values so this situation (from has value and to is empty, is detected as a deletion)
                    if (string.IsNullOrWhiteSpace(toItem.Value))
                    {
                        operation=ModelIndexOperationEnum.Delete;
                    }

                    result.Add(new ModelIndexComparisonResult(item, toItem, operation));
                }
            }

            return result;
        }
    }
}
