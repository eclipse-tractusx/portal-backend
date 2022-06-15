using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for persistence layer access relating <see cref="Company"/> entities.
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Retrieves company entity from persistence layer.
    /// </summary>
    /// <param name="companyId">Id of the company to retrieve.</param>
    /// <returns>Requested company entity or null if it does not exist.</returns>
    public ValueTask<Company?> GetCompanyByIdAsync(Guid companyId);
}
