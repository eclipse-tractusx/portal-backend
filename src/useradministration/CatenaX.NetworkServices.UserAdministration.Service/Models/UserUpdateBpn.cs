using System.Collections.Generic;
namespace CatenaX.NetworkServices.UserAdministration.Service.Models
{
    public class UserUpdateBpn
    {
        public string userId { get; set; }

        public IEnumerable<string> bpns { get; set; }
    }
}
