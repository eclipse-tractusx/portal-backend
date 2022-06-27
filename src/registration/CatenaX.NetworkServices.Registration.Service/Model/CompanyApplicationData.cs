using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Registration.Service.Model;

public class CompanyApplicationData
{
    public Guid ApplicationId { get; set; }

    public CompanyApplicationStatusId? ApplicationStatus { get; set; }
}
