using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Registration.Service.Bpn;

/// <summary>
/// Service to call the BPDM endpoints
/// </summary>
public interface IBpdmService
{
    /// <summary>
    /// Triggers the bpn data push
    /// </summary>
    /// <param name="data">The bpdm data</param>
    /// <param name="accessToken">The access token to call the service</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Returns <c>true</c> if the service call was successful, otherwise <c>false</c></returns>
    Task<bool> TriggerBpnDataPush(BpdmData data, string accessToken, CancellationToken cancellationToken);
}