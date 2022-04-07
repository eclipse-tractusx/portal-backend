using System;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities
{
    public class AppAssignedLicense
    {
        public AppAssignedLicense() {}
        public AppAssignedLicense(App app, AppLicense appLicense)
        {
            App = app;
            AppLicense = appLicense;
        }

        public Guid AppId { get; set; }
        public Guid AppLicenseId { get; set; }

        public virtual App App { get; set; }
        public virtual AppLicense AppLicense { get; set; }
    }
}
