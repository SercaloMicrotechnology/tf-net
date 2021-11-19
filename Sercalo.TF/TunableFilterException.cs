using System;

namespace Sercalo.TF
{
    [Serializable]
    public class TunableFilterException : SercaloException
    {
        public ErrorCode Code { get; private set; }

        public TunableFilterException(ErrorCode code = ErrorCode.Unknown)
            : this(code.GetDescription(), code) { }
        public TunableFilterException(string message, ErrorCode code = ErrorCode.Unknown) : base(message)
        {
            Code = code;
        }
        public TunableFilterException(string message, Exception inner, ErrorCode code = ErrorCode.Unknown) : base(message, inner)
        {
            Code = code;
        }
        protected TunableFilterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
