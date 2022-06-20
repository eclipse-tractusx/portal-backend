namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models
{
    public class UploadDocuments
    {
        public UploadDocuments(Guid documentId, string documentName)
        {
            DocumentId = documentId;
            DocumentName = documentName;
        }
        public Guid DocumentId { get;}
        
        public string DocumentName { get;}
    }

    public class RegistrationDocumentNames
    {
        public RegistrationDocumentNames(string documentName)
        {
            DocumentName = documentName;
        }
    
        public string DocumentName { get; set;}
    }
    
}
