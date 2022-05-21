using Lendsum.Crosscutting.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.UnitOfWork
{
    internal class FromToTypes
    {
        // type is the model type
        // string is the instance name
        // FromTo is the model loaded from db, and the model modified (to)
        private Dictionary<Type, FromToInstances> map = new Dictionary<Type, FromToInstances>();
        public FromToInstances GetByType<T>() { return this.GetByType(typeof(T)); }

        public FromToInstances GetByType(Type type) { return map.GetOrAdd(type, () => new FromToInstances()); }

        public void SetFrom<T>(T? from, string instanceName = "") where T:AggregateModelBase
        {
            var target = this.GetByType<T>().GetByInstanceName(instanceName);
            target.From = from;
            target.FromSet = true;
        }

        public void SetTo<T>(T? to, string instanceName) where T:AggregateModelBase
        {
            var target = this.GetByType<T>().GetByInstanceName(instanceName);
            target.To = to;
            target.ToSet = true;
        }

        public IEnumerable<Type> GetAll()
        {
            return this.map.Keys;
        }

    }

    internal class FromToInstances
    {
        private Dictionary<string, FromToModels> map = new Dictionary<string, FromToModels>();

        public FromToModels GetByInstanceName(string instanceName = "")
        {
            return map.GetOrAdd(instanceName, () => new FromToModels());
        }
        public IEnumerable<string> GetAll()
        {
            return this.map.Keys;
        }
    }

    internal class FromToModels
    {
        public bool FromSet { get; set; }
        public AggregateModelBase? From { get; set; }
        public bool ToSet { get; set; }
        public AggregateModelBase? To { get; set; }
    }
}
