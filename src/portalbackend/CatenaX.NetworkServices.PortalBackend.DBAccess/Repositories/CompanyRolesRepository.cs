using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public class CompanyRolesRepository : ICompanyRolesRepository
{
    private readonly PortalDbContext _dbContext;

    private CompanyRolesRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public Consent CreateConsent(Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, string? Comment = null, string? Target = null, Guid? DocumentId = null) =>
        _dbContext.Consents.Add(
            new Consent(
                Guid.NewGuid(),
                agreementId,
                companyId,
                companyUserId,
                consentStatusId,
                DateTimeOffset.UtcNow)
            {
                Comment = Comment,
                Target = Target,
                DocumentId = DocumentId
            }).Entity;

    public CompanyAssignedRole CreateCompanyAssignedRole(Guid companyId, CompanyRoleId companyRoleId) =>
        _dbContext.CompanyAssignedRoles.Add(
            new CompanyAssignedRole(
                companyId,
                companyRoleId
            )).Entity;

    public CompanyAssignedRole RemoveCompanyAssignedRole(CompanyAssignedRole companyAssignedRole) =>
        _dbContext.Remove(companyAssignedRole).Entity;

    public Task<CompanyRoleAgreementConsentData?> GetCompanyRoleAgreementConsentDataAsync(Guid applicationId, string iamUserId) =>
        _dbContext.CompanyApplications
            .Where(application => application.Id == applicationId)
            .Select(application => new CompanyRoleAgreementConsentData(
                application.Company!.CompanyUsers.Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId).Select(companyUser => companyUser.Id).SingleOrDefault(),
                application.CompanyId,
                application,
                application.Company.CompanyAssignedRoles,
                application.Company.Consents.Where(consent => consent.ConsentStatusId == ConsentStatusId.ACTIVE)))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<AgreementsAssignedCompanyRoleData> GetAgreementAssignedCompanyRolesUntrackedAsync(IEnumerable<CompanyRoleId> companyRoleIds) =>
        _dbContext.CompanyRoles
            .AsNoTracking()
            .Where(companyRole => companyRoleIds.Contains(companyRole.Id))
            .Select(companyRole => new AgreementsAssignedCompanyRoleData(
                companyRole.Id,
                companyRole.AgreementAssignedCompanyRoles!.Select(agreementAssignedCompanyRole => agreementAssignedCompanyRole.AgreementId)
            )).AsAsyncEnumerable();

    public Task<CompanyRoleAgreementConsents?> GetCompanyRoleAgreementConsentStatusUntrackedAsync(Guid applicationId, string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId
                && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
            .Select(iamUser => iamUser.CompanyUser!.Company)
            .Select(company => new CompanyRoleAgreementConsents(
                company!.CompanyAssignedRoles.Select(companyAssignedRole => companyAssignedRole.CompanyRoleId),
                company.Consents.Where(consent => consent.ConsentStatusId == PortalBackend.PortalEntities.Enums.ConsentStatusId.ACTIVE).Select(consent => new AgreementConsentStatus(
                    consent.AgreementId,
                    consent.ConsentStatusId
                )))).SingleOrDefaultAsync();

    public async IAsyncEnumerable<CompanyRoleData> GetCompanyRoleAgreementsUntrackedAsync()
    {
        await foreach (var role in _dbContext.CompanyRoles
            .AsNoTracking()
            .Select(companyRole => new
            {
                Id = companyRole.Id,
                Descriptions = companyRole.CompanyRoleDescriptions.Select(description => new { ShortName = description.LanguageShortName, Description = description.Description }),
                Agreements = companyRole.AgreementAssignedCompanyRoles.Select(agreementAssignedCompanyRole => agreementAssignedCompanyRole.AgreementId)
            })
            .AsAsyncEnumerable())
        {
            yield return new CompanyRoleData(
                role.Id,
                role.Descriptions.ToDictionary(d => d.ShortName, d => d.Description),
                role.Agreements);
        }
    }

    public IAsyncEnumerable<AgreementData> GetAgreementsUntrackedAsync() =>
        _dbContext.Agreements
            .AsNoTracking()
            .Select(agreement => new AgreementData(
                agreement.Id,
                agreement.Name))
            .AsAsyncEnumerable();
}
