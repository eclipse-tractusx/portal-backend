using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface ICompanySsiDetailsRepository
{
    IAsyncEnumerable<UseCaseParticipationData> GetUseCaseParticipationForCompany(Guid companyId, string language);
    IAsyncEnumerable<SsiCertificateData> GetSsiCertificates(Guid companyId);
}
