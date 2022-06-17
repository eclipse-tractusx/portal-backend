using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Framework.Models;
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
    public Task<Pagination.Response<CompanyApplicationDetails>> GetApplicationDetailsAsync([FromQuery]int page, [FromQuery]int size) =>
        _logic.GetCompanyApplicationDetailsAsync(page, size);

    [HttpPut]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/approveRequest")]
    public Task<bool> ApprovePartnerRequest([FromRoute] Guid applicationId) =>
            _logic.ApprovePartnerRequest(applicationId);

    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("applications/{companyName}")]
    public Task<Pagination.Response<CompanyApplicationDetails>> GetApplicationByCompanyNameAsync([FromRoute]string companyName, [FromQuery]int page, [FromQuery]int size) =>
        _logic.GetApplicationByCompanyNameAsync(companyName, page, size);
}
