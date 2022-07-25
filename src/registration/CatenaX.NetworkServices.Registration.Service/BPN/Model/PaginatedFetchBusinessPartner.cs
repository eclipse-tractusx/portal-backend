using System.Collections.Generic;

namespace CatenaX.NetworkServices.Registration.Service.BPN.Model
{
    public class PaginatedFetchBusinessPartner
    {
        public int pageSize { get; set; }
        public int totals { get; set; }
        public int page { get; set; }
        public IEnumerable<FetchBusinessPartnerDto> values { get; set; }
    }
}



