using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IConnectorsBusinessLogic"/> making use of <see cref="IConnectorsRepository"/> to retrieve data.
/// </summary>
public class ConnectorsBusinessLogic : IConnectorsBusinessLogic
{
    private readonly IConnectorsRepository _repository;
    private readonly ConnectorsSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="connectorsRepository">Connectors repository.</param>
    public ConnectorsBusinessLogic(IConnectorsRepository connectorsRepository, IOptions<ConnectorsSettings> options)
    {
        this._repository = connectorsRepository;
        this._settings = options.Value;
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<ConnectorViewModel>> GetAllCompanyConnectorViewModelsForIamUserAsyncEnum(string iamUserId, int page, int size) =>
        Pagination.CreateResponseAsync(page, size, _settings.MaxPageSize, (skip, take) =>
            new Pagination.AsyncSource<ConnectorViewModel>
            (
                this._repository.GetAllCompanyConnectorsForIamUser(iamUserId).CountAsync(),
                this._repository.GetAllCompanyConnectorsForIamUser(iamUserId)
                    .Skip(skip)
                    .Take(take)
                    .Select(c => 
                        new ConnectorViewModel(c.Name, c.Location!.Alpha2Code)
                        {
                            Id = c.Id,
                            Status = c.Status!.Id,
                            Type = c.Type!.Id
                        }
                    ).AsAsyncEnumerable()
            )
        );
}
