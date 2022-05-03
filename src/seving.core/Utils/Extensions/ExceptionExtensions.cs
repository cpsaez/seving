using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Utils.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception AddData(this Exception ex, string key, string value)
        {
            ex.AddData(key, value); 
            return ex;
        }

        public static Exception AddStreamRootUid(this Exception ex, Guid uid)
        {
            return ex.AddData("StreamRootUid", uid.ToString());
        }

        public static Exception AddStreamRootVersion(this Exception ex, int version)
        {
            return ex.AddData("StreamRootUid", version.ToString(CultureInfo.InvariantCulture));
        }
    }
}
