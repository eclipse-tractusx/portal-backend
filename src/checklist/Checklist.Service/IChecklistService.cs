namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Service;

public interface IChecklistService
{
    /// <summary>
    /// Triggers the bpn data push
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    /// <param name="iamUserId">the current user</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    Task TriggerBpnDataPush(Guid applicationId, string iamUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Updates the company bpn
    /// </summary>
    /// <param name="applicationId">Id of the application to update the bpn</param>
    /// <param name="bpn">the bpn to set</param>
    Task UpdateCompanyBpn(Guid applicationId, string bpn);
}
