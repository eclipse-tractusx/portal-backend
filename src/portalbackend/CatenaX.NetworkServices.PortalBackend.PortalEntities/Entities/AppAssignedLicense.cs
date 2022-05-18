namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AppAssignedLicense
{
    private AppAssignedLicense() {}

    public AppAssignedLicense(Guid appId, Guid appLicenseId)
    {
        AppId = appId;
        AppLicenseId = appLicenseId;
    }

    public Guid AppId { get; private set; }
    public Guid AppLicenseId { get; private set; }

    // Navigation properties
    public virtual App? App { get; set; }
    public virtual AppLicense? AppLicense { get; set; }
}
