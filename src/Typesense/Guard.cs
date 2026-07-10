using System;

namespace Typesense;

internal static class Guard
{
    public static void ThrowIfNull(object? argument, string paramName)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);
    }
}
