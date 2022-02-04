using System;

namespace Typesense;

public class TypesenseApiException : Exception
{
    public TypesenseApiException() { }
    public TypesenseApiException(string message) : base(message) { }
    public TypesenseApiException(string message, Exception innerException) : base(message, innerException) { }
}
