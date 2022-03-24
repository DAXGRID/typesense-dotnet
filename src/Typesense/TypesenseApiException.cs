using System;

namespace Typesense;

public class TypesenseApiException : Exception
{
    public TypesenseApiException() { }
    public TypesenseApiException(string message) : base(message) { }
    public TypesenseApiException(string message, Exception innerException) : base(message, innerException) { }
}

public class TypesenseApiBadRequestException : TypesenseApiException
{
    public TypesenseApiBadRequestException() { }
    public TypesenseApiBadRequestException(string message) : base(message) { }
    public TypesenseApiBadRequestException(string message, Exception innerException) : base(message, innerException) { }
}

public class TypesenseApiUnauthorizedException : TypesenseApiException
{
    public TypesenseApiUnauthorizedException() { }
    public TypesenseApiUnauthorizedException(string message) : base(message) { }
    public TypesenseApiUnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
}

public class TypesenseApiNotFoundException : TypesenseApiException
{
    public TypesenseApiNotFoundException() { }
    public TypesenseApiNotFoundException(string message) : base(message) { }
    public TypesenseApiNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

public class TypesenseApiConflictException : TypesenseApiException
{
    public TypesenseApiConflictException() { }
    public TypesenseApiConflictException(string message) : base(message) { }
    public TypesenseApiConflictException(string message, Exception innerException) : base(message, innerException) { }
}

public class TypesenseApiUnprocessableEntityException : TypesenseApiException
{
    public TypesenseApiUnprocessableEntityException() { }
    public TypesenseApiUnprocessableEntityException(string message) : base(message) { }
    public TypesenseApiUnprocessableEntityException(string message, Exception innerException) : base(message, innerException) { }
}

public class TypesenseApiServiceUnavilableException : TypesenseApiException
{
    public TypesenseApiServiceUnavilableException() { }
    public TypesenseApiServiceUnavilableException(string message) : base(message) { }
    public TypesenseApiServiceUnavilableException(string message, Exception innerException) : base(message, innerException) { }
}
