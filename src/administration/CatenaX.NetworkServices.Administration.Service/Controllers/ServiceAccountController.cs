using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Provisioning.Library.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/serviceaccount")]
public class ServiceAccountController : ControllerBase
{
    private readonly IServiceAccountBusinessLogic _logic;
    public ServiceAccountController(IServiceAccountBusinessLogic logic)
    {
        _logic = logic;
    }

    [HttpPost]
    [Authorize(Roles = "add_tech_user_management")]
    [Route("owncompany/serviceaccounts")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status201Created)]
    public async Task<ActionResult<ServiceAccountDetails>> ExecuteCompanyUserCreation([FromBody] ServiceAccountCreationInfo serviceAccountCreationInfo)
    {
        var serviceAccountDetails = await this.WithIamUserId(createdByName => _logic.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfo, createdByName).ConfigureAwait(false));
        return CreatedAtRoute("GetServiceAccountDetails", new { serviceAccountId = serviceAccountDetails.ServiceAccountId }, serviceAccountDetails);
    }

    [HttpDelete]
    [Authorize(Roles = "delete_tech_user_management")]
    [Route("owncompany/serviceaccounts/{serviceAccountId}")]
    public Task<int> DeleteServiceAccount([FromRoute] Guid serviceAccountId) =>
        this.WithIamUserId(adminId => _logic.DeleteOwnCompanyServiceAccountAsync(serviceAccountId, adminId));

    [HttpGet]
    [Authorize(Roles = "view_tech_user_management")]
    [Route("owncompany/serviceaccounts/{serviceAccountId}", Name="GetServiceAccountDetails")]
    public Task<ServiceAccountDetails> GetServiceAccountDetails([FromRoute] Guid serviceAccountId) =>
        this.WithIamUserId(adminId => _logic.GetOwnCompanyServiceAccountDetailsAsync(serviceAccountId, adminId));

    [HttpPut]
    [Authorize(Roles = "add_tech_user_management")] // TODO check whether we also want an edit role
    [Route("owncompany/serviceaccounts/{serviceAccountId}")]
    public Task<ServiceAccountDetails> PutServiceAccountDetails([FromRoute] Guid serviceAccountId, [FromBody] ServiceAccountEditableDetails serviceAccountDetails) =>
        this.WithIamUserId(adminId => _logic.UpdateOwnCompanyServiceAccountDetailsAsync(serviceAccountId, serviceAccountDetails, adminId));

    [HttpGet]
    [Authorize(Roles = "view_tech_user_management")]
    [Route("owncompany/serviceaccounts")]
    public Task<Pagination.Response<ServiceAccountData>> GetServiceAccountsData([FromQuery] int page, [FromQuery] int size) =>
        this.WithIamUserId(adminId => _logic.GetOwnCompanyServiceAccountsDataAsync(page, size, adminId));
}
