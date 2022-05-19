using System;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Registration.Service.Model
{

    public class UploadDocumentDetails
    {
        public UploadDocumentDetails(Guid documentId, string documentName)
        {
            DocumentId = documentId;
            DocumentName = documentName;
        }
        public Guid DocumentId { get; set; }
        public string DocumentName { get; set; }
    }
}
