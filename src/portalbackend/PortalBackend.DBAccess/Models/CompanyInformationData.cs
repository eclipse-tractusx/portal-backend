namespace PortalBackend.DBAccess.Models;

public record CompanyInformationData(
    Guid CompanyId,
    string OrganizationName,
    string Country,
    string? BusinessPartnerNumber);