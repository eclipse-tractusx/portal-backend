using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Provisioning.Library.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/serviceaccount")]
[Produces("application/json")]
[Consumes("application/json")]
public class ServiceAccountController : ControllerBase
{
    private readonly IServiceAccountBusinessLogic _logic;
    
    /// <summary>
    /// Creates a new instance of <see cref="ServiceAccountController"/> 
    /// </summary>
    /// <param name="logic">The Service Account Buisness Logic</param>
    public ServiceAccountController(IServiceAccountBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceAccountCreationInfo"></param>
    /// <returns></returns>
    /// Example: POST: api/administration/serviceaccount/owncompany/serviceaccounts
    /// <response code="201">The service account was created.</response>
    /// <response code="400">Missing mandatory input values (e.g. name) or not supported authenticationType selected.</response>
    /// <response code="404">Record was not found. Possible reason: invalid user role, requester user invalid.</response>
    [HttpPost]
    [Authorize(Roles = "add_tech_user_management")]
    [Route("owncompany/serviceaccounts")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceAccountDetails>> ExecuteCompanyUserCreation([FromBody] ServiceAccountCreationInfo serviceAccountCreationInfo)
    {
        var serviceAccountDetails = await this.WithIamUserId(createdByName => _logic.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfo, createdByName).ConfigureAwait(false));
        return CreatedAtRoute("GetServiceAccountDetails", new { serviceAccountId = serviceAccountDetails.ServiceAccountId }, serviceAccountDetails);
    }

    /// <summary>
    /// Deletes the service account with the given id
    /// </summary>
    /// <param name="serviceAccountId" example="7e85a0b8-0001-ab67-10d1-0ef508201000">Id of the service account that should be deleted.</param>
    /// <returns></returns>
    /// Example: DELETE: api/administration/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000
    /// <response code="200">Successful if the service account was deleted.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    [HttpDelete]
    [Authorize(Roles = "delete_tech_user_management")]
    [Route("owncompany/serviceaccounts/{serviceAccountId}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<int> DeleteServiceAccount([FromRoute] Guid serviceAccountId) =>
        this.WithIamUserId(adminId => _logic.DeleteOwnCompanyServiceAccountAsync(serviceAccountId, adminId));

    /// <summary>
    /// Gets the service account details for the given id
    /// </summary>
    /// <param name="serviceAccountId">Id to get the service account details for.</param>
    /// <returns>Returns a list of service account details.</returns>
    /// Example: GET: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000
    /// <response code="200">Returns a list of service account details.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    [HttpGet]
    [Authorize(Roles = "view_tech_user_management")]
    [Route("owncompany/serviceaccounts/{serviceAccountId}", Name="GetServiceAccountDetails")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceAccountDetails> GetServiceAccountDetails([FromRoute] Guid serviceAccountId) =>
        this.WithIamUserId(adminId => _logic.GetOwnCompanyServiceAccountDetailsAsync(serviceAccountId, adminId));

    /// <summary>
    /// Updates the service account details with the given id.
    /// </summary>
    /// <param name="serviceAccountId">Id of the service account details that should be updated.</param>
    /// <param name="serviceAccountDetails">The new values for the details.</param>
    /// <returns>Returns the updated service account details.</returns>
    /// Example: PUT: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000
    /// <response code="200">Returns the updated service account details.</response>
    /// <response code="400">
    /// Problem could be one of the following: <br />
    /// - other authenticationType values than SECRET are not supported yet <br />
    /// - serviceAccountId from path does not match the one in body <br />
    /// - serviceAccount is already INACTIVE <br />
    /// </response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    [HttpPut]
    [Authorize(Roles = "add_tech_user_management")] // TODO check whether we also want an edit role
    [Route("owncompany/serviceaccounts/{serviceAccountId}")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceAccountDetails> PutServiceAccountDetails([FromRoute] Guid serviceAccountId, [FromBody] ServiceAccountEditableDetails serviceAccountDetails) =>
        this.WithIamUserId(adminId => _logic.UpdateOwnCompanyServiceAccountDetailsAsync(serviceAccountId, serviceAccountDetails, adminId));

    /// <summary>
    /// Resets the service account credentials for the given service account Id.
    /// 
    /// </summary>
    /// <param name="serviceAccountId">Id of the service account.</param>
    /// <returns>Returns the service account details.</returns>
    /// Example: PUT: api/administration/serviceaccount/owncompany/serviceaccounts/7e85a0b8-0001-ab67-10d1-0ef508201000/resetCredentials
    /// <response code="200">Returns the service account details.</response>
    /// <response code="404">Record was not found. Service account is either not existing or not connected to the respective company.</response>
    [HttpPost]
    [Authorize(Roles = "add_tech_user_management")]
    [Route("owncompany/serviceaccounts/{serviceAccountId}/resetCredentials")]
    [ProducesResponseType(typeof(ServiceAccountDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceAccountDetails> ResetServiceAccountCredentials([FromRoute] Guid serviceAccountId) =>
        this.WithIamUserId(adminId => _logic.ResetOwnCompanyServiceAccountSecretAsync(serviceAccountId, adminId));

    /// <summary>
    /// Gets the service account data as pagination
    /// </summary>
    /// <param name="page">the page of service account data</param>
    /// <param name="size">number of service account data</param>
    /// <returns>Returns the specific number of service account data for the given page.</returns>
    /// Example: GET: api/administration/serviceaccount/owncompany/serviceaccounts
    /// <response code="200">Returns the specific number of service account data for the given page.</response>
    [HttpGet]
    [Authorize(Roles = "view_tech_user_management")]
    [Route("owncompany/serviceaccounts")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyServiceAccountData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CompanyServiceAccountData>> GetServiceAccountsData([FromQuery] int page, [FromQuery] int size) =>
        this.WithIamUserId(adminId => _logic.GetOwnCompanyServiceAccountsDataAsync(page, size, adminId));
}
