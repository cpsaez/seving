//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using Lendsum.Crosscutting.Common.Extensions;
//using Newtonsoft.Json.Serialization;
//#pragma warning disable CS8601 // Possible null reference assignment.
//#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
//namespace seving.core.Utils.Serializer
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <seealso cref="Newtonsoft.Json.Serialization.ISerializationBinder" />
//    public class CurrentKnownTypesBinder : ISerializationBinder
//    {
//        /// <summary>
//        /// The types dictionary
//        /// This dictionary has the types that have been deserialized
//        /// If the type is not present when deserializing, the type that will be saved is object
//        /// </summary>
//        private static Dictionary<string, Type> typesDictionary = new Dictionary<string, Type>();
        
//        /// <summary>
//        /// When implemented, controls the binding of a serialized object to a type.
//        /// </summary>
//        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
//        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
//        /// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
//        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
//        {
//            serializedType = serializedType?? throw new ArgumentException( nameof(serializedType));
//            assemblyName = serializedType.Assembly.GetName().Name;
//            typeName = serializedType.FullName;

//        }

//        /// <summary>
//        /// When implemented, controls the binding of a serialized object to a type.
//        /// </summary>
//        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
//        /// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object</param>
//        /// <returns>
//        /// The type of the object the formatter creates a new instance of.
//        /// </returns>
//        public Type BindToType(string assemblyName, string typeName)
//        {
//            string typeAndAssembly = typeName + ", " + assemblyName;
//            if (typesDictionary.TryGetValue(typeAndAssembly, out Type typeToReturn))
//            {
//                return typeToReturn;
//            }
//            else
//            {
//                var actualTypeToReturn = Type.GetType(typeName + ","  +  assemblyName);

//                lock (typesDictionary)
//                {
//                    if (!typesDictionary.TryGetValue(typeAndAssembly, out typeToReturn))
//                    {
//                        if (actualTypeToReturn != null)
//                        {
//                            typesDictionary.Add(typeAndAssembly, actualTypeToReturn);
//                            return actualTypeToReturn;
//                        }

//                        typesDictionary.Add(typeAndAssembly, typeof(object));
//                        return typeof(object);
//                    }
//                    else
//                    {
//                        return typeToReturn;
//                    }

//                }
//            }
//        }

//    }
//}

//#pragma warning restore CS8601 // Possible null reference assignment.
//#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.