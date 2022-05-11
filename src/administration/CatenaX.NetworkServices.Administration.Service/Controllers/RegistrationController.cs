using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/registration")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationBusinessLogic _logic;
    public RegistrationController(IRegistrationBusinessLogic logic)
    {
        _logic = logic;
    }

    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("application/{applicationId}/companyDetailsWithAddress")]
    [ProducesResponseType(typeof(CompanyWithAddress), (int)HttpStatusCode.OK)]
    public Task<CompanyWithAddress> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
        _logic.GetCompanyWithAddressAsync(applicationId);

    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("applications")]
    public IAsyncEnumerable<CompanyApplicationDetails> GetApplicationDetailsAsync([FromQuery]int page) =>
        _logic.GetCompanyApplicationDetailsAsync(page);

    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("applications/pages")]
    public Task<PaginationData> GetApplicationPaginationDataAsync() =>
        _logic.GetApplicationPaginationDataAsync();
}
