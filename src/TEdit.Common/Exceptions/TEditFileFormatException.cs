using System;

namespace TEdit.Common.Exceptions;

public class TEditFileFormatException : Exception
{
    public TEditFileFormatException()
    {
    }

    public TEditFileFormatException(
        string message) : base(message)
    {
    }

    public TEditFileFormatException(
        string message, 
        Exception innerException) : base(message, innerException)
    {
    }

    protected TEditFileFormatException(
        System.Runtime.Serialization.SerializationInfo info, 
        System.Runtime.Serialization.StreamingContext context) : base(info, context)
    {
    }
}
