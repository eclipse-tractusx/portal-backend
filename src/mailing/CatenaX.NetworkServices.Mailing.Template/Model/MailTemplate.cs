namespace CatenaX.NetworkServices.Mailing.Template.Model
{
    public class MailTemplate
    {
        public MailTemplate(string id, string subject, string body)
        {
            Id = id;
            Subject = subject;
            Body = body;
        }
        
        public string Id { get; }
        public string Subject { get; }
        public string Body { get; }
    }
}
