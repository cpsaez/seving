using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Persistence
{
    public class BatchQuery<T> where T:IPersistable
    {
        public BatchQuery()
        {
            this.Items = Enumerable.Empty<T>();
            this.StartKey = String.Empty;
            this.EndKey = String.Empty;
            this.Partition = String.Empty;
            this.Limit = null;
            this.IncludeKeys = false;
            this.Ascendent = true;
            this.ConstantSegment = String.Empty;
        }

        public IEnumerable<T> Items { get; set; }
        public string StartKey { get; set; }
        public string EndKey { get; set; }
        public string Partition { get; set; }
        public string ConstantSegment { get; set; }
        public int? Limit { get; set; }
        public bool IncludeKeys { get; set; }
        public bool Ascendent { get; set; }

        public BatchQuery<T> Advance(IEnumerable<T> items)
        {
            string lastKey = this.EndKey;
            var lastItem = items.LastOrDefault();
            if (lastItem!=null)
            {
                lastKey = lastItem.Keys.Key;
            }

            var result = new BatchQuery<T>()
            {
                Ascendent = this.Ascendent,
                IncludeKeys = this.IncludeKeys,
                Limit = this.Limit,
                EndKey = this.EndKey,
                StartKey = lastKey,
                Items = items,
                ConstantSegment = this.ConstantSegment,
                Partition = this.Partition
            };

            return result;
        }
    }
}
