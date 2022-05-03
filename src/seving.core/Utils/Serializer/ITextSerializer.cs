using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Utils.Serializer
{
    /// <summary>
    /// Interface for object-to-text serializer
    /// </summary>
    public interface ITextSerializer
    {
        /// <summary>
        /// Serializes a supplied object instance to proper serialized text (JSON, XML,...)
        /// determine by function implemenation
        /// </summary>
        /// <param name="o">Instance to be serialized</param>
        /// <returns>Returns string in which object is serialized. Type of serialization is determine by
        /// function implementation (JSON, XML,...)</returns>
        string Serialize(object o);

        /// <summary>
        /// Serializes a supplied instance od specified type to proper serialized text (JSON, XML,...)
        /// determine by function implemenation
        /// </summary>
        /// <typeparam name="T">Type of the object which is being serialized</typeparam>
        /// <param name="o">Instance to be serialized</param>
        /// <param name="typeNameHandlingAll">if set to <c>true</c> the types will be specified in the output.</param>
        /// <returns>
        /// Returns string in which object is serialized. Type of serialization is determine by
        /// function implementation (JSON, XML,...)
        /// </returns>
        string Serialize<T>(T o, bool typeNameHandlingAll = true);

        /// <summary>
        /// Deserializes an object from supplied text.
        /// </summary>
        /// <param name="oSerialized">String value from which object is being deserialized</param>
        /// <param name="type">Type of the object which is being serialized</param>
        /// <returns>The object deserialized.</returns>
        object Deserialize(string oSerialized, Type type);

        /// <summary>
        /// Deserializes an object from supplied text.
        /// </summary>
        /// <typeparam name="T">Type of the object which is being serialized</typeparam>
        /// <param name="oSerialized">String value from which object is being deserialized</param>
        /// <returns>The object deserialized.</returns>
        T Deserialize<T>(string oSerialized);
    }
}
