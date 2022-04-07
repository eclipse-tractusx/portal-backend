using System;
using System.Net;

namespace CatenaX.NetworkServices.Registration.Service.CustomException
{
    public class ServiceException: Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public ServiceException(string message, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) : base(message)
        {
            StatusCode = httpStatusCode;
        }
    }
}
