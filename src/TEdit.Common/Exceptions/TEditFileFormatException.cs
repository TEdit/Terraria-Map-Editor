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

#pragma warning disable SYSLIB0051 // Obsolete serialization constructor required for netstandard2.0 compatibility
    protected TEditFileFormatException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context)
    {
    }
#pragma warning restore SYSLIB0051
}
