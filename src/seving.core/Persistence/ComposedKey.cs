using Lendsum.Crosscutting.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Persistence
{
    /// <summary>
    /// Represents a composed key
    /// </summary>
    public class ComposedKey
    {
        public ComposedKey()
        {
            this.Keys = Enumerable.Empty<string>();
        }

        public ComposedKey(Guid key1)
        {
            this.Keys = new string[] { key1.ToString() };
        }

        public ComposedKey(string key1)
        {
            this.Keys = new string[] { key1 };
        }

        public ComposedKey(Guid key1, int key2)
        {
            this.Keys = new string[] { key1.ToString(), key2.ToString("D8", CultureInfo.InvariantCulture) };
        }

        public ComposedKey(string key1, string key2)
        {
            this.Keys = new string[] { key1, key2 };
        }

        public ComposedKey(string key1, Guid key2)
        {
            this.Keys = new string[] { key1, key2.ToString() };
        }

        public ComposedKey(string key1, string key2, string key3)
        {
            this.Keys = new string[] { key1, key2, key3 };
        }

        public ComposedKey(string key1, string key2, Guid key3)
        {
            this.Keys = new string[] { key1, key2, key3.ToString() };
        }

        public ComposedKey(Guid key1, string key2, int key3) : this(key1)
        {
            this.Keys = new string[] { key1.ToString(), key2, key3.ToString("D8", CultureInfo.InvariantCulture) };
        }

        public IEnumerable<string> Keys { get; set; }

        public string Key => Keys.ToSentence(":");
    }
}
