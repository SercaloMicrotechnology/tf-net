using System;

namespace Sercalo
{
    /// <summary>
    /// Represents errors that occurs in Sercalo Library
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class SercaloException : Exception
    {
        /// <summary>
        /// Gets the error number (if know, 0 otherwise)
        /// </summary>
        public int ErrorNumber { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SercaloException"/> class.
        /// </summary>
        /// <param name="errorNumber">The error number.</param>
        public SercaloException(int errorNumber = 0)
        {
            ErrorNumber = errorNumber;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SercaloException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorNumber">The error number.</param>
        public SercaloException(string message, int errorNumber = 0) : base(message)
        {
            ErrorNumber = errorNumber;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SercaloException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="errorNumber">The error number.</param>
        public SercaloException(string message, Exception inner, int errorNumber = 0) : base(message, inner)
        {
            ErrorNumber = errorNumber;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SercaloException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        /// <param name="errorNumber">The error number.</param>
        protected SercaloException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context, 
          int errorNumber = 0) : base(info, context)
        {
            ErrorNumber = errorNumber;
        }
    }
}
