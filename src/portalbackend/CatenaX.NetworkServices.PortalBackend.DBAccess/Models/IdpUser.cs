namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class IdpUser
    {
        public IdpUser(string targetIamUserid, string idpName)
        {
            TargetIamUserId = targetIamUserid;
            IdpName = idpName;
        }
        public string TargetIamUserId { get; set; }
        public string IdpName { get; set; }
    }
}