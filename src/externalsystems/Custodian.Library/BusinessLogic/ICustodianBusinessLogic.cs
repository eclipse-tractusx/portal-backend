namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;

public interface ICustodianBusinessLogic
{
    Task<string> CreateWalletAsync(Guid applicationId, CancellationToken cancellationToken);
}