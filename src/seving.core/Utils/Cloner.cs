using seving.core.Utils.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Utils
{
    public static class Cloner
    {
        static JsonTextSerializer serializer=new JsonTextSerializer();
        public static T Clone<T>(Object o)
        {
            var raw = serializer.Serialize(o);
            T result=serializer.Deserialize<T>(raw);
            return result;
        }

        public static bool AreEquals<T>(T a, T b)
        {
            return serializer.Serialize(a) == serializer.Serialize(b);
        }
    }
}
