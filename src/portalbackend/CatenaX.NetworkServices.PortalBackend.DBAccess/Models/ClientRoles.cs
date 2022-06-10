
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class ClientRoles
    {
        public ClientRoles(Guid roleId, string role, string description)
        {
            RoleId = roleId;
            Role = role;
            Description = description;
        }

        public Guid RoleId { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
    }
}
