using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface ICompanySsiDetailsRepository
{
    IAsyncEnumerable<UseCaseParticipationTransferData> GetUseCaseParticipationForCompany(Guid companyId, string language);
    IAsyncEnumerable<SsiCertificateTransferData> GetSsiCertificates(Guid companyId);
}
