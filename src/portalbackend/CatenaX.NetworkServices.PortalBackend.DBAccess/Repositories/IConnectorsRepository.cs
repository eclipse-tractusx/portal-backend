using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing connectors on persistence layer.
/// </summary>
public interface IConnectorsRepository
{
    /// <summary>
    /// Get all connectors of a user's company by iam user ID.
    /// </summary>
    /// <param name="iamUserId">ID of the iam user used to determine company's connectors for.</param>
    /// <returns>Queryable of connectors that allows transformation.</returns>
    public IQueryable<Connector> GetAllCompanyConnectorsForIamUser(string iamUserId);

    /// <summary>
    /// Creates a given connector in persistence layer. 
    /// </summary>
    /// <param name="connector">Connector to create.</param>
    /// <returns>Created and persisted connector.</returns>
    public Task<Connector> CreateConnectorAsync(Connector connector);

    /// <summary>
    /// Removes a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId">ID of the connector to be deleted.</param>
    public Task DeleteConnectorAsync(Guid connectorId);
}
