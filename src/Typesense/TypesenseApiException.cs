using System;

namespace Typesense;

/// <summary>
/// TypesenseApiException general TypesenseApiException.
/// </summary>
public class TypesenseApiException : Exception
{
    public TypesenseApiException() { }
    public TypesenseApiException(string message) : base(message) { }
    public TypesenseApiException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Bad Request - The request could not be understood due to malformed syntax.
/// </summary>
public class TypesenseApiBadRequestException : TypesenseApiException
{
    public TypesenseApiBadRequestException() { }
    public TypesenseApiBadRequestException(string message) : base(message) { }
    public TypesenseApiBadRequestException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Unauthorized - Your API key is wrong.
/// </summary>
public class TypesenseApiUnauthorizedException : TypesenseApiException
{
    public TypesenseApiUnauthorizedException() { }
    public TypesenseApiUnauthorizedException(string message) : base(message) { }
    public TypesenseApiUnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Not Found - The requested resource is not found.
/// </summary>
public class TypesenseApiNotFoundException : TypesenseApiException
{
    public TypesenseApiNotFoundException() { }
    public TypesenseApiNotFoundException(string message) : base(message) { }
    public TypesenseApiNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Conflict - When a resource already exists.
/// </summary>
public class TypesenseApiConflictException : TypesenseApiException
{
    public TypesenseApiConflictException() { }
    public TypesenseApiConflictException(string message) : base(message) { }
    public TypesenseApiConflictException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Unprocessable Entity - Request is well-formed, but cannot be processed.
/// </summary>
public class TypesenseApiUnprocessableEntityException : TypesenseApiException
{
    public TypesenseApiUnprocessableEntityException() { }
    public TypesenseApiUnprocessableEntityException(string message) : base(message) { }
    public TypesenseApiUnprocessableEntityException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Service Unavailable - We’re temporarily offline. Please try again later.
/// </summary>
public class TypesenseApiServiceUnavailableException : TypesenseApiException
{
    public TypesenseApiServiceUnavailableException() { }
    public TypesenseApiServiceUnavailableException(string message) : base(message) { }
    public TypesenseApiServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
