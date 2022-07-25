using System;

namespace CatenaX.NetworkServices.Mailing.Template
{
    public class NoSuchTemplateException : Exception
    {
        public NoSuchTemplateException(string message) : base(message)
        {
        }
    }
}
