using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sercalo
{
    [Serializable]
    public class SercaloException : Exception
    {
        public SercaloException() { }
        public SercaloException(string message) : base(message) { }
        public SercaloException(string message, Exception inner) : base(message, inner) { }
        protected SercaloException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
