using System.Collections.Generic;
namespace CatenaX.NetworkServices.Administration.Service.Models
{
    public class UserUpdateBpn
    {
        public string userId { get; set; }

        public IEnumerable<string> bpns { get; set; }
    }
}
