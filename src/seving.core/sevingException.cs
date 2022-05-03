using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seving.core
{
    /// <summary>
    /// Exception from seving framework
    /// </summary>
    public class SevingException : Exception 
    {
        public SevingException(string message):base(message)
        {
        }
    }
}
