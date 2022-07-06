using System.Net;

namespace CatenaX.NetworkServices.Framework.ErrorHandling;

[Serializable]
public class ServiceException : Exception
{
    public HttpStatusCode StatusCode { get; set; }

    public ServiceException(string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) : base(message)
    {
        StatusCode = httpStatusCode;
    }
    public ServiceException(string message, System.Exception inner, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) : base(message, inner)
    {
        StatusCode = httpStatusCode;
    }

    protected ServiceException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
