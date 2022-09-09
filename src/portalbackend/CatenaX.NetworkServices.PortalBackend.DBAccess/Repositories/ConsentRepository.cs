using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class ConsentRepository : IConsentRepository
{
    private readonly PortalDbContext _portalDbContext;

    /// <summary>
    /// Creates an instance of <see cref="ConsentRepository"/>
    /// </summary>
    /// <param name="portalDbContext">The database</param>
    public ConsentRepository(PortalDbContext portalDbContext)
    {
        _portalDbContext = portalDbContext;
    }

    /// <inheritdoc/>
    public Consent CreateConsent(Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, Action<Consent>? setupOptionalFields)
    {
        var consent = new Consent(Guid.NewGuid(), agreementId, companyId, companyUserId, consentStatusId, DateTimeOffset.UtcNow);
        setupOptionalFields?.Invoke(consent);
        return _portalDbContext.Consents.Add(consent).Entity;
    }

    /// <inheritdoc />
    public void AttachToDatabase(IEnumerable<Consent> consents) => 
        _portalDbContext.AttachRange(consents.ToArray());

    /// <inheritdoc />
    public void RemoveConsents(IEnumerable<Consent> consents) => 
        _portalDbContext.RemoveRange(consents);

    /// <inheritdoc />
    public Task<ConsentDetailData?> GetConsentDetailData(Guid consentId) =>
        _portalDbContext.Consents
            .Where(x => x.Id == consentId)
            .Select(x => new ConsentDetailData(
                x.Id,
                x.Company!.Name,
                x.CompanyUserId,
                x.ConsentStatusId,
                x.Agreement!.Name
            ))
            .SingleOrDefaultAsync();
}
