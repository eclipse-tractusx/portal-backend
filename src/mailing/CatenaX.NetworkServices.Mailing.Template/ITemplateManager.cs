using System;
using System.Collections.Generic;
using CatenaX.NetworkServices.Mailing.Template.Model;

namespace CatenaX.NetworkServices.Mailing.Template
{
    public interface ITemplateManager
    {
        Mail ApplyTemplate(String Id, IDictionary<string,string> Parameters);
    }
}
