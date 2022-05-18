using System;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Registration.Service.Model
{
    public class UploadDocumentDetails
    {
        public Guid DocumentId { get; set; }
        public string DocumentName { get; set; }
    }
}
