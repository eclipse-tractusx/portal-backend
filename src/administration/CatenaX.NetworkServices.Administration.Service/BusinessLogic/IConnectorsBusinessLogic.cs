using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Business logic for handling connector api requests.
/// </summary>
public interface IConnectorsBusinessLogic
{
    /// <summary>
    /// Get all of a user's company's connectors by iam user ID.
    /// </summary>
    /// <param name="iamUserId">ID of the user to retrieve company connectors for.</param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns>AsyncEnumerable of the result connectors.</returns>
    public Task<Pagination.Response<ConnectorViewModel>> GetAllCompanyConnectorViewModelsForIamUserAsyncEnum(string iamUserId, int page, int size);

    /// <summary>
    /// Add a connector to persistence layer.
    /// </summary>
    /// <param name="connectorInputModel">Connector parameters for creation.</param>
    /// <returns>View model of created connector.</returns>
    public Task<ConnectorViewModel> CreateConnectorAsync(ConnectorInputModel connectorInputModel);

    /// <summary>
    /// Remove a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId">ID of the connector to be deleted.</param>
    public Task DeleteConnectorAsync(Guid connectorId);
}
