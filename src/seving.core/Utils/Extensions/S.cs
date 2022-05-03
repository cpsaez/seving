using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core.Utils.Extensions
{    public static class S
    {
        /// <summary>
        /// Easy way to use $
        /// string text = Invariant($"{p.Name} was born on {p.DateOfBirth:D}");
        /// </summary>
        /// <param name="formattable">The formattable.</param>
        /// <returns></returns>
        public static string? Invariant(FormattableString formattable)
        {
            if (formattable == null) return null;
            return formattable.ToString(CultureInfo.InvariantCulture);
        }
    }
}
