using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;

public class CustodianBusinessLogic : ICustodianBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICustodianService _custodianService;

    public CustodianBusinessLogic(IPortalRepositories portalRepositories, ICustodianService custodianService)
    {
        _portalRepositories = portalRepositories;
        _custodianService = custodianService;
    }
    
    /// <inheritdoc />
    public async Task<string> CreateWalletAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyAndApplicationDetailsForSubmittedApplicationAsync(applicationId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }
        var (companyId, companyName, businessPartnerNumber, _) = result;

        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ControllerArgumentException($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyId} is empty", "bpn");
        }
        
        return await _custodianService.CreateWalletAsync(businessPartnerNumber, companyName, cancellationToken).ConfigureAwait(false);
    }
}