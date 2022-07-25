using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class CompanyApplicationWithStatus
    {
        public Guid ApplicationId;
        public CompanyApplicationStatusId? ApplicationStatus; //FIXME - this should not be nullable!
    }
}
