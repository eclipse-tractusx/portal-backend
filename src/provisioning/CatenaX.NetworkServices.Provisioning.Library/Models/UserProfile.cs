namespace CatenaX.NetworkServices.Provisioning.Library.Models
{
    public class UserProfile
    {
        public UserProfile(string userName, string email, string organisationName)
        {
            UserName = userName;
            Email = email;
            OrganisationName = organisationName;
        }
        public string UserName;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email;
        public string? Password { get; set; }
        public string OrganisationName;
        public string? BusinessPartnerNumber { get; set; }
    }
}
