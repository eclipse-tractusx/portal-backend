using System.Collections.Generic;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class CompanyToRoles
    {
        public string CompanyId { get; set; }

        public IEnumerable<int> roles { get; set; }
    }
}
