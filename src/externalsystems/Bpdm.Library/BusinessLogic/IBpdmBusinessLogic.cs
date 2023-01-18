namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;

public interface IBpdmBusinessLogic
{
    /// <summary>
    /// Triggers the bpn data push
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <param name="iamUserId">Id of the current user</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Returns <c>true</c> if the service call was successful, otherwise <c>false</c></returns>
    Task<bool> TriggerBpnDataPush(Guid applicationId, string iamUserId, CancellationToken cancellationToken);
}