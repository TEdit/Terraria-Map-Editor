using System;
using System.Runtime.Serialization;

namespace TEdit.Common.Exceptions
{
    [Serializable]
    public class TEditFileFormatException : Exception
    {
        public TEditFileFormatException()
        {
        }

        public TEditFileFormatException(string message) : base(message)
        {
        }

        public TEditFileFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TEditFileFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
