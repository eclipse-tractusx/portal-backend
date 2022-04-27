using System;

namespace CatenaX.NetworkServices.Framework.ErrorHandling
{
    [Serializable]
    public class ForbiddenException : Exception
    {
        public ForbiddenException() { }
        public ForbiddenException(string message) : base(message) { }
        public ForbiddenException(string message, System.Exception inner) : base(message, inner) { }
        protected ForbiddenException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}