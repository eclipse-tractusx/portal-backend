namespace CatenaX.NetworkServices.Mailing.Template.Model
{
    public class Mail
    {
        public Mail(string subject, string body, bool html)
        {
            Subject = subject;
            Body = body;
            isHtml = html;
        }
        public string Subject { get; }
        public string Body { get; }
        public bool isHtml { get; }
    }
}
