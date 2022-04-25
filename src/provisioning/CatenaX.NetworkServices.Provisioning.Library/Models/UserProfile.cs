namespace CatenaX.NetworkServices.Provisioning.Library.Models
{
    public class UserProfile
    {
        public UserProfile(string UserName, string FirstName, string LastName, string Email, string? Password = null)
        {
            this.UserName = UserName;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.Email = Email;
        }
        public string UserName;
        public string FirstName;
        public string LastName;
        public string Email;
        public string? Password;
    }
}
