using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface ICompanyRolesRepository
{
    CompanyAssignedRole CreateCompanyAssignedRole(Guid companyId, CompanyRoleId companyRoleId);
    CompanyAssignedRole RemoveCompanyAssignedRole(CompanyAssignedRole companyAssignedRole);
    Task<CompanyRoleAgreementConsentData?> GetCompanyRoleAgreementConsentDataAsync(Guid applicationId, string iamUserId);
    IAsyncEnumerable<AgreementsAssignedCompanyRoleData> GetAgreementAssignedCompanyRolesUntrackedAsync(IEnumerable<CompanyRoleId> companyRoleIds);
    Task<CompanyRoleAgreementConsents?> GetCompanyRoleAgreementConsentStatusUntrackedAsync(Guid applicationId, string iamUserId);
    IAsyncEnumerable<CompanyRoleData> GetCompanyRoleAgreementsUntrackedAsync();
    IAsyncEnumerable<CompanyRolesDetails> GetCompanyRolesAsync(string? languageShortName = null);
}
