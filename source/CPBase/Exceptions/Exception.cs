
using System;
using System.Collections.Generic;

namespace Contensive.Exceptions {
    /// <summary>
    /// Non-specific exception. Used during code conversion. Do not add to future code.
    /// </summary>
    [Serializable] public class GenericException : ApplicationException {
        /// <summary>
        /// Raise generic exception
        /// </summary>
        /// <param name="message"></param>
        public GenericException(string message) : base(message) { }
        /// <summary>
        /// Raise generic exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public GenericException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>
        /// Raise generic exception
        /// </summary>
        public GenericException() : base() {
        }
        /// <summary>
        /// Raise generic exception
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        protected GenericException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) {
            throw new NotImplementedException();
        }
    }
}
