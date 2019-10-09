using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GvLib
{
    class IncorrectGvFileContentsException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public IncorrectGvFileContentsException() { }

        public IncorrectGvFileContentsException(string message) : base(message) { }

        public IncorrectGvFileContentsException(string message, Exception innerException) : base(message, innerException) { }

        protected IncorrectGvFileContentsException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override string ToString() => $"Исключение IncorrectGvFileContentsException: {Message}";
    }
}
