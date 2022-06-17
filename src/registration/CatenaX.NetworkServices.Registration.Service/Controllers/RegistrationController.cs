using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Registration.Service.BusinessLogic;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Net;
using CatenaX.NetworkServices.Registration.Service.BPN.Model;

namespace CatenaX.NetworkServices.Registration.Service.Controllers
{
    [ApiController]
    [Route("api/registration")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly IRegistrationBusinessLogic _registrationBusinessLogic;

        /// <summary>
        /// Creates a new instance of <see cref="RegistrationController"/>
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="registrationBusinessLogic">Access to the business logic</param>
        public RegistrationController(ILogger<RegistrationController> logger, IRegistrationBusinessLogic registrationBusinessLogic)
        {
            _logger = logger;
            _registrationBusinessLogic = registrationBusinessLogic;
        }

        /// <summary>
        /// Gets a company by its bpn
        /// </summary>
        /// <param name="bpn" example="CAXSDUMMYCATENAZZ">The bpn to get the company for</param>
        /// <param name="authorization">the authorization</param>
        /// <returns>Returns a List with one company</returns>
        /// <remarks>Example: Get: /api/registration/company/{bpn}CAXSDUMMYCATENAZZ</remarks>
        /// <response code="200">Returns the company</response>
        /// <response code="400">The requested service responded with the given error.</response>
        [HttpGet]
        [Authorize(Roles = "add_company_data")]
        [Route("company/{bpn}")]
        [ProducesResponseType(typeof(List<FetchBusinessPartnerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOneObjectAsync([FromRoute] string bpn, [FromHeader] string authorization)
        {
            try
            {
                return Ok(await _registrationBusinessLogic.GetCompanyByIdentifierAsync(bpn, authorization.Split(" ")[1]).ConfigureAwait(false));
            }
            catch (ServiceException e)
            {
                var content = new { message = e.Message };
                return new ContentResult { StatusCode = (int)e.StatusCode, Content = JsonConvert.SerializeObject(content), ContentType = "application/json" };
            }
        }

        /// <summary>
        /// Uploads a document
        /// </summary>
        /// <param name="applicationId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645"></param>
        /// <param name="documentTypeId" example="1"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <remarks>Example: Post: /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents</remarks>
        /// <response code="200">Successfully uploaded the document</response>
        /// <response code="403">The user is not assigned with the CompanyAppication.</response>
        [HttpPost]
        [Authorize(Roles = "upload_documents")]
        [Route("application/{applicationId}/documentType/{documentTypeId}/documents")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public Task<int> UploadDocumentAsync([FromRoute] Guid applicationId, [FromRoute] DocumentTypeId documentTypeId, [FromForm(Name = "document")] IFormFile document) =>
            this.WithIamUserId(user => _registrationBusinessLogic.UploadDocumentAsync(applicationId, document, documentTypeId, user));

        /// <summary>
        /// Gets documents for a specific document type and application
        /// </summary>
        /// <param name="applicationId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">The application to get the documents from.</param>
        /// <param name="documentTypeId" example="1">Type of the documents to get.</param>
        /// <returns>Returns a list of documents.</returns>
        /// <remarks>Example: Get: /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents</remarks>
        /// <response code="200">Returns a list of documents</response>
        /// <response code="403">The user is not associated with invitation</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("application/{applicationId}/documentType/{documentTypeId}/documents")]
        [ProducesResponseType(typeof(IAsyncEnumerable<UploadDocuments> ), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync([FromRoute] Guid applicationId,[FromRoute] DocumentTypeId documentTypeId) =>
           this.WithIamUserId(user => _registrationBusinessLogic.GetUploadedDocumentsAsync(applicationId,documentTypeId,user));

        /// <summary>
        /// Sets the idp to the given company
        /// </summary>
        /// <param name="idpToSet">Information to set the idp</param>
        /// <returns>Returns OK</returns>
        /// <remarks>Example: Put: /api/registration/idp</remarks>
        /// <response code="200">Successfully set the idp</response>
        [HttpPut]
        [Authorize(Roles = "invite_user")]
        [Route("idp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SetIdpAsync([FromBody] SetIdp idpToSet)
        {
            await _registrationBusinessLogic.SetIdpAsync(idpToSet).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Returns a list of composite names
        /// </summary>
        /// <returns>Returns a list of composite names</returns>
        /// <remarks>Example: Get: /api/registration/rolesComposite</remarks>
        /// <response code="200">Returns a list of composite names</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("rolesComposite")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClientRolesComposite()
        {
            try
            {
                var result = await _registrationBusinessLogic.GetClientRolesCompositeAsync().ConfigureAwait(false);
                return Ok(result.ToList());

            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets the applications with each status
        /// </summary>
        /// <returns>Returns a list of company applications</returns>
        /// <remarks>Example: Get: /api/registration/applications</remarks>
        /// <response code="200">Returns a list of company applications</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("applications")]
        [ProducesResponseType(typeof(IAsyncEnumerable<CompanyApplication>), StatusCodes.Status200OK)]
        public IAsyncEnumerable<CompanyApplication> GetApplicationsWithStatusAsync() =>
            this.WithIamUserId(user => _registrationBusinessLogic.GetAllApplicationsForUserWithStatus(user));

        /// <summary>
        /// Sets the status of a specific application.
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application which status should be set.</param>
        /// <param name="status" example="8">The status that should be set</param>
        /// <returns></returns>
        /// <remarks>Example: Put: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/status</remarks>
        /// <response code="200">Successfully set the status</response>
        /// <response code="404">CompanyApplication was not found for the given id.</response>
        [HttpPut]
        [Authorize(Roles = "submit_registration")]
        [Route("application/{applicationId}/status")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task<int> SetApplicationStatusAsync([FromRoute] Guid applicationId, [FromQuery] CompanyApplicationStatusId status) =>
            _registrationBusinessLogic.SetApplicationStatusAsync(applicationId, status);

        /// <summary>
        /// Gets the status of an application for the given id
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application to get the status from</param>
        /// <returns>Returns the company application status</returns>
        /// <remarks>Example: Get: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/status</remarks>
        /// <response code="200">Returns the company application status</response>
        /// <response code="404">CompanyApplication was not found for the given id.</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("application/{applicationId}/status")]
        [ProducesResponseType(typeof(CompanyApplicationStatusId), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task<CompanyApplicationStatusId> GetApplicationStatusAsync([FromRoute] Guid applicationId) =>
            _registrationBusinessLogic.GetApplicationStatusAsync(applicationId);

        /// <summary>
        /// Gets the company of a specific application with its address
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application to get the company to.</param>
        /// <returns>Returns the company with its address</returns>
        /// <remarks>Example: Get: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/companyDetailsWithAddress</remarks>
        /// <response code="200">Returns the company with its address</response>
        /// <response code="404">CompanyApplication was not found for the given id.</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("application/{applicationId}/companyDetailsWithAddress")]
        [ProducesResponseType(typeof(CompanyWithAddress), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task<CompanyWithAddress> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
            _registrationBusinessLogic.GetCompanyWithAddressAsync(applicationId);

        /// <summary>
        /// Sets the company with its address for the given application id
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application to set the company for.</param>
        /// <param name="companyWithAddress">The company with its address</param>
        /// <remarks>Example: Post: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/companyDetailsWithAddress</remarks>
        /// <response code="200">Successfully set the company with its address</response>
        /// <response code="404">CompanyApplication was not found for the given id.</response>
        [HttpPost]
        [Authorize(Roles = "add_company_data")]
        [Route("application/{applicationId}/companyDetailsWithAddress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task SetCompanyWithAddressAsync([FromRoute] Guid applicationId, [FromBody] CompanyWithAddress companyWithAddress) =>
            _registrationBusinessLogic.SetCompanyWithAddressAsync(applicationId, companyWithAddress);

        /// <summary>
        /// Invites the given user to the given application
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application a user should be invited to</param>
        /// <param name="userCreationInfo">The information of the user that should be invited</param>
        /// <returns></returns>
        /// <remarks>Example: Post: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/inviteNewUser</remarks>
        /// <response code="200">Successfully invited the user.</response>
        /// <response code="403">Either the user was not found or the user is not assigneable to the given application.</response>
        /// <response code="404">The shared idp was not found  for the CompanyApplication.</response>
        [HttpPost]
        [Authorize(Roles = "invite_user")]
        [Route("application/{applicationId}/inviteNewUser")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
        public Task<int> InviteNewUserAsync([FromRoute] Guid applicationId, [FromBody] UserCreationInfo userCreationInfo) =>
            this.WithIamUserId(iamUserId =>
                _registrationBusinessLogic.InviteNewUserAsync(applicationId, userCreationInfo, iamUserId));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4"></param>
        /// <param name="companyRolesAgreementConsents"></param>
        /// <returns></returns>
        /// <remarks>Example: Post: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/companyRoleAgreementConsents</remarks>
        /// <response code="200">Successfully submitted consent to agreements</response>
        /// <response code="403">Either the user was not found or the user is not assignable to the given application.</response>
        [HttpPost]
        [Authorize(Roles = "submit_registration")]
        [Route("application/{applicationId}/companyRoleAgreementConsents")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public Task<int> SubmitCompanyRoleConsentToAgreementsAsync([FromRoute] Guid applicationId, [FromBody] CompanyRoleAgreementConsents companyRolesAgreementConsents) =>
            this.WithIamUserId(iamUserId =>
                _registrationBusinessLogic.SubmitRoleConsentAsync(applicationId, companyRolesAgreementConsents, iamUserId));

        /// <summary>
        /// Gets the agreement consent statuses for the given application
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application to get the agreement consent statuses for</param>
        /// <returns>Return the company role agreement consents</returns>
        /// <remarks>Example: Get: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/companyRoleAgreementConsents</remarks>
        /// <response code="200">Return the company role agreement consents</response>
        /// <response code="403">The user is not assignable to the given application.</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("application/{applicationId}/companyRoleAgreementConsents")]
        [ProducesResponseType(typeof(CompanyRoleAgreementConsents), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public Task<CompanyRoleAgreementConsents> GetAgreementConsentStatusesAsync([FromRoute] Guid applicationId) =>
            this.WithIamUserId(iamUserId =>
                _registrationBusinessLogic.GetRoleAgreementConsentsAsync(applicationId, iamUserId));

        /// <summary>
        /// Gets the company role agreement data
        /// </summary>
        /// <returns>Returns the Company role agreement data</returns>
        /// <remarks>Example: Get: /api/registration/companyRoleAgreementData</remarks>
        /// <response code="200">Returns the Company role agreement data</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("companyRoleAgreementData")]
        [ProducesResponseType(typeof(CompanyRoleAgreementData), StatusCodes.Status200OK)]
        public Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync() =>
            _registrationBusinessLogic.GetCompanyRoleAgreementDataAsync();

        /// <summary>
        /// Submits a registration
        /// </summary>
        /// <returns>Returns ok</returns>
        /// <remarks>Example: Post: /api/registration/submitregistration</remarks>
        /// <response code="200">Successfully submitted the registration</response>
        [HttpPost]
        [Authorize(Roles = "submit_registration")]
        [Route("submitregistration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SubmitRegistrationAsync()
        {
            try
            {
                var userEmail = User.Claims.SingleOrDefault(x => x.Type == "email").Value as string;

                if (await _registrationBusinessLogic.SubmitRegistrationAsync(userEmail).ConfigureAwait(false))
                {
                    return Ok();
                }
                _logger.LogError("unsuccessful");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Gets all invited users for a given application
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application</param>
        /// <remarks>Example: Get: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/invitedusers</remarks>
        /// <returns>Returns all invited users</returns>
        /// <response code="200">Returns all invited users</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("application/{applicationId}/invitedusers")]
        [ProducesResponseType(typeof(IAsyncEnumerable<InvitedUser>), StatusCodes.Status200OK)]
        public IAsyncEnumerable<InvitedUser> GetInvitedUsersAsync([FromRoute] Guid applicationId) =>
            _registrationBusinessLogic.GetInvitedUsersAsync(applicationId);

        /// <summary>
        /// Sets the invitation status
        /// </summary>
        /// <returns></returns>
        /// <remarks>Example: Put: /api/registration/invitation/status</remarks>
        /// <response code="200">Successfully sets the invitation status</response>
        /// <response code="403">The user id is not associated with invitation.</response>
        [HttpPut]
        [Authorize(Roles = "view_registration")]
        [Route("invitation/status")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public Task<int> SetInvitationStatusAsync() =>
           this.WithIamUserId(iamUserId =>
                _registrationBusinessLogic.SetInvitationStatusAsync(iamUserId));

        /// <summary>
        /// Gets the registration data for the given application id
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">The application id to get the registration data for.</param>
        /// <returns>Returns the registration data</returns>
        /// <remarks>Example: Get: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/registrationData</remarks>
        /// <response code="200">Returns the registration data</response>
        /// <response code="403">The user id is not associated with CompanyApplication.</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("application/{applicationId}/registrationData")]
        [ProducesResponseType(typeof(RegistrationData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
         public Task<RegistrationData> GetRegistrationDataAsync([FromRoute] Guid applicationId) =>
            this.WithIamUserId(iamUserId => 
                _registrationBusinessLogic.GetRegistrationDataAsync(applicationId,iamUserId));
    }
}
