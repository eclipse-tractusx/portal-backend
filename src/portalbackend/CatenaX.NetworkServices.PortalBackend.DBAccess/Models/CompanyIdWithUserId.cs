using System;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyIdWithUserId
    {
        public CompanyIdWithUserId(Guid companyId, Guid userId)
        {
            CompanyId = companyId;
            CompanyUserId = userId;
        }

        public Guid CompanyId { get; }
        public Guid CompanyUserId { get; }
    }
}