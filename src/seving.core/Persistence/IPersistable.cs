using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Persistence
{
    /// <summary>
    /// Interface to be implemented by any object that must be saved in any database or blob storage
    /// </summary>
    public interface IPersistable
    {
        /// <summary>
        /// Partition where this data will be saved
        /// </summary>
        string Partition { get; }

        /// <summary>
        /// Key under this object will be saved.
        /// </summary>
        ComposedKey Keys { get; }

        /// <summary>
        /// Concurrency control attribute
        /// </summary>
        string? Cas { get; set; }
    }
}
