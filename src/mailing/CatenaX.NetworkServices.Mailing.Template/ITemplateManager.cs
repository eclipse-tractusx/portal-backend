using CatenaX.NetworkServices.Mailing.Template.Model;

namespace CatenaX.NetworkServices.Mailing.Template
{
    public interface ITemplateManager
    {
        Mail ApplyTemplate(string id, IDictionary<string,string> parameters);
    }
}
