using System;

namespace Typesense
{
    public class TypesenseApiException : Exception
    {
        public TypesenseApiException(string message) : base(message)
        {
        }
    }
}
