using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

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
}
