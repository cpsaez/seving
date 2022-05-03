using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace seving.core.Utils.Serializer
{
    /// <summary>
    /// Json object serializer
    /// </summary>
    public class JsonTextSerializer : ITextSerializer
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            //SerializationBinder = new CurrentKnownTypesBinder()
        };

        /// <summary>
        /// Serializes a supplied object instance to JSON
        /// </summary>
        /// <param name="o">Instance to be serialized</param>
        /// <returns>
        /// Returns string of JSON serialization.
        /// </returns>
        public string Serialize(object o)
        {
            o = o ?? throw new ArgumentNullException(nameof(o));
            string output = JsonConvert.SerializeObject(o, Formatting.Indented, settings);
            return output.Replace("System.Private.CoreLib", "mscorlib");
        }

        /// <summary>
        /// Serializes a supplied object instance to JSON
        /// </summary>
        /// <typeparam name="T">Type of the object which is being serialized to JSON</typeparam>
        /// <param name="o">Instance to be serialized</param>
        /// <param name="typeNameHandlingAll">if set to <c>true</c> the types will be specified.</param>
        /// <returns>
        /// Returns string of JSON serialization.
        /// </returns>
        public string Serialize<T>(T o, bool typeNameHandlingAll = true)
        {
            o = o ?? throw new ArgumentNullException(nameof(o));

            if (typeNameHandlingAll)
            {
                var output = JsonConvert.SerializeObject(o, typeof(T), Formatting.Indented, settings);
                return output;//.Replace("System.Private.CoreLib", "mscorlib");
            }
            else
            {
                var output = JsonConvert.SerializeObject(o);
                return output;//.Replace("System.Private.CoreLib", "mscorlib"); ;
            }
        }

        /// <summary>
        /// Deserializes a supplied object
        /// </summary>
        /// <param name="oSerialized">String value from which object is being deserialized</param>
        /// <returns>The deserialized object.</returns>
        public dynamic Deserialize(string oSerialized)
        {
            if (string.IsNullOrWhiteSpace(oSerialized))
            {
                throw new ArgumentNullException(nameof(oSerialized));
            }

            return JObject.Parse(oSerialized);
        }

        /// <summary>
        /// Deserializes a supplied object
        /// </summary>
        /// <param name="oSerialized">String value from which object is being deserialized</param>
        /// <param name="type">Type of the object which is being serialized</param>
        /// <returns>The deserialized object.</returns>
        public object Deserialize(string oSerialized, Type type)
        {
            if (string.IsNullOrWhiteSpace(oSerialized))
            {
                throw new ArgumentNullException(nameof(oSerialized));
            }

            if (type == null) throw new ArgumentNullException(nameof(type));

            var result = JsonConvert.DeserializeObject(oSerialized, type, this.settings);

            if (result == null) throw new JsonSerializationException("The oSerialized string cannot be deserialized because the result is null");

            return result;
        }

        /// <summary>
        /// Deserializes the specified o serialized.
        /// </summary>
        /// <typeparam name="T">Type of the object which is being serialized</typeparam>
        /// <param name="oSerialized">The o serialized.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string oSerialized)
        {
            if (string.IsNullOrWhiteSpace(oSerialized))
            {
                throw new ArgumentNullException(nameof(oSerialized));
            }

            var result = JsonConvert.DeserializeObject<T>(oSerialized, this.settings);

            if (result == null) throw new JsonSerializationException("The oSerialized string cannot be deserialized because the result is null");

            return result;
        }
    }
}