﻿using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

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

    /// <inheritdoc />
    public void AttachToDatabase(IEnumerable<Consent> consents) => _portalDbContext.AttachRange(consents.ToArray());

    /// <inheritdoc />
    public void RemoveConsents(IEnumerable<Consent> consents)
    {
        throw new NotImplementedException();
    }
}
