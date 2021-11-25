using System;

namespace Sercalo.TF
{
    /// <summary>
    /// Represents errors that occurs during an operation of a Tunable Filter
    /// </summary>
    /// <seealso cref="Sercalo.SercaloException" />
    [Serializable]
    public class TunableFilterException : SercaloException
    {
        /// <summary>
        /// Gets the code of the error
        /// </summary>
        public ErrorCode Code => (ErrorCode)ErrorNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="TunableFilterException"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        public TunableFilterException(ErrorCode code = ErrorCode.Unknown)
            : this(code.GetDescription(), code) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TunableFilterException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="code">The code.</param>
        public TunableFilterException(string message, ErrorCode code = ErrorCode.Unknown) : base(message, (int)code) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TunableFilterException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="code">The code.</param>
        public TunableFilterException(string message, Exception inner, ErrorCode code = ErrorCode.Unknown) : base(message, inner, (int)code) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TunableFilterException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        /// <param name="code">The code.</param>
        protected TunableFilterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context, 
          ErrorCode code = ErrorCode.Unknown) : base(info, context, (int)code) { }
    }
}
