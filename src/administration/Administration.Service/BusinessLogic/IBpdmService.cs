using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Service to call the BPDM endpoints
/// </summary>
public interface IBpdmService
{
    /// <summary>
    /// Triggers the bpn data push
    /// </summary>
    /// <param name="data">The bpdm data</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Returns <c>true</c> if the service call was successful, otherwise <c>false</c></returns>
    Task<bool> TriggerBpnDataPush(BpdmTransferData data, CancellationToken cancellationToken);
}