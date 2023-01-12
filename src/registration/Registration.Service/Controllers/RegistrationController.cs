/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers
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
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Returns a List with one company</returns>
        /// <remarks>Example: Get: /api/registration/company/{bpn}CAXSDUMMYCATENAZZ</remarks>
        /// <response code="200">Returns the company</response>
        /// <response code="503">The requested service responded with the given error.</response>
        [HttpGet]
        [Authorize(Roles = "add_company_data")]
        [Route("company/{bpn}")]
        [ProducesResponseType(typeof(IAsyncEnumerable<FetchBusinessPartnerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
        public IAsyncEnumerable<FetchBusinessPartnerDto> GetOneObjectAsync([FromRoute] string bpn, [FromHeader] string authorization, CancellationToken cancellationToken) => 
            _registrationBusinessLogic.GetCompanyByIdentifierAsync(bpn, authorization.Split(" ")[1], cancellationToken);

        /// <summary>
        /// Uploads a document
        /// </summary>
        /// <param name="applicationId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645"></param>
        /// <param name="documentTypeId" example="1"></param>
        /// <param name="document"></param>
        /// <param name="cancellationToken">CancellationToken (provided by controller)</param>
        /// <returns></returns>
        /// <remarks>Example: Post: /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents</remarks>
        /// <response code="200">Successfully uploaded the document</response>
        /// <response code="403">The user is not assigned with the CompanyApplication.</response>
        /// <response code="415">Only PDF files are supported.</response>
        [HttpPost]
        [Authorize(Roles = "upload_documents")]
        [Consumes("multipart/form-data")]
        [Route("application/{applicationId}/documentType/{documentTypeId}/documents")]
        [RequestFormLimits(ValueLengthLimit = 819200, MultipartBodyLengthLimit = 819200)]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
        public Task<int> UploadDocumentAsync([FromRoute] Guid applicationId, [FromRoute] DocumentTypeId documentTypeId, [FromForm(Name = "document")] IFormFile document, CancellationToken cancellationToken) =>
            this.WithIamUserId(user => _registrationBusinessLogic.UploadDocumentAsync(applicationId, document, documentTypeId, user, cancellationToken));

        /// <summary>
        /// Gets a specific document by its id
        /// </summary>
        /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3"></param>
        /// <returns></returns>
        /// <remarks>Example: Post: /api/registration/documents/4ad087bb-80a1-49d3-9ba9-da0b175cd4e3</remarks>
        /// <response code="200">Successfully uploaded the document</response>
        /// <response code="403">User does not have the relevant rights to request for the document.</response>
        /// <response code="404">No document with the given id was found.</response>
        [HttpGet]
        [Authorize(Roles = "view_documents")]
        [Route("documents/{documentId}")]
        [Produces("application/pdf", "application/json")]
        [ProducesResponseType(typeof(File), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetDocumentContentFileAsync([FromRoute] Guid documentId)
        {
            var (fileName, content) = await this.WithIamUserId(user => _registrationBusinessLogic.GetDocumentContentAsync(documentId, user));
            return File(content, "application/pdf", fileName);
        }

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
        public Task<IEnumerable<UploadDocuments>> GetUploadedDocumentsAsync([FromRoute] Guid applicationId,[FromRoute] DocumentTypeId documentTypeId) =>
           this.WithIamUserId(user => _registrationBusinessLogic.GetUploadedDocumentsAsync(applicationId, documentTypeId, user));

        /// <summary>
        /// Get all composite client roles
        /// </summary>
        /// <returns>all composite client roles</returns>
        /// <remarks>Example: Get: /api/registration/rolesComposite</remarks>
        /// <response code="200">returns all composite client roles</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("rolesComposite")]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        public IAsyncEnumerable<string> GetClientRolesComposite() =>
            _registrationBusinessLogic.GetClientRolesCompositeAsync();

        /// <summary>
        /// Gets the applications with each status
        /// </summary>
        /// <returns>Returns a list of company applications</returns>
        /// <remarks>Example: Get: /api/registration/applications</remarks>
        /// <response code="200">Returns a list of company applications</response>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("applications")]
        [ProducesResponseType(typeof(IAsyncEnumerable<CompanyApplicationData>), StatusCodes.Status200OK)]
        public IAsyncEnumerable<CompanyApplicationData> GetApplicationsWithStatusAsync() =>
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
            this.WithIamUserId(user => _registrationBusinessLogic.SetOwnCompanyApplicationStatusAsync(applicationId, status, user));

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
            this.WithIamUserId(user => _registrationBusinessLogic.GetOwnCompanyApplicationStatusAsync(applicationId, user));

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
        /// <response code="400">A request parameter was incorrect.</response>
        /// <response code="404">CompanyApplication was not found for the given id.</response>
        [HttpPost]
        [Authorize(Roles = "add_company_data")]
        [Route("application/{applicationId}/companyDetailsWithAddress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public Task SetCompanyWithAddressAsync([FromRoute] Guid applicationId, [FromBody] CompanyWithAddress companyWithAddress) =>
            this.WithIamUserId(iamUserId =>
                _registrationBusinessLogic.SetCompanyWithAddressAsync(applicationId, companyWithAddress, iamUserId));

        /// <summary>
        /// Invites the given user to the given application
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application a user should be invited to</param>
        /// <param name="userCreationInfo">The information of the user that should be invited</param>
        /// <returns></returns>
        /// <remarks>Example: Post: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/inviteNewUser</remarks>
        /// <response code="200">Successfully invited the user.</response>
        /// <response code="400">The user with the given email does already exist.</response>
        /// <response code="403">Either the user was not found or the user is not assigneable to the given application.</response>
        /// <response code="404">The shared idp was not found  for the CompanyApplication.</response>
        [HttpPost]
        [Authorize(Roles = "invite_user")]
        [Route("application/{applicationId}/inviteNewUser")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status404NotFound)]
        public Task<int> InviteNewUserAsync([FromRoute] Guid applicationId, [FromBody] UserCreationInfoWithMessage userCreationInfo) =>
            this.WithIamUserId(iamUserId =>
                _registrationBusinessLogic.InviteNewUserAsync(applicationId, userCreationInfo, iamUserId));

        /// <summary>
        /// Post the agreement consent status for the given application.
        /// </summary>
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4"></param>
        /// <param name="companyRolesAgreementConsents"></param>
        /// <returns></returns>
        /// <remarks>Example: Post: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/companyRoleAgreementConsents</remarks>
        /// <response code="200">Successfully submitted consent to agreements</response>
        /// <response code="403">Either the user was not found or the user is not assignable to the given application.</response>
        [HttpPost]
        [Authorize(Roles = "sign_consent")]
        [Route("application/{applicationId}/companyRoleAgreementConsents")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public Task<int> SubmitCompanyRoleConsentToAgreementsAsync([FromRoute] Guid applicationId, [FromBody] CompanyRoleAgreementConsents companyRolesAgreementConsents) =>
            this.WithIamUserId(iamUserId => _registrationBusinessLogic.SubmitRoleConsentAsync(applicationId, companyRolesAgreementConsents, iamUserId));

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
        /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application to submit registration</param>
        /// <returns>Returns ok</returns>
        /// <remarks>Example: Post: /api/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/submitRegistration</remarks>
        /// <response code="200">Successfully submitted the registration</response>
        [HttpPost]
        [Authorize(Roles = "submit_registration")]
        [Route("application/{applicationId}/submitRegistration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<bool> SubmitRegistrationAsync([FromRoute] Guid applicationId) =>
            this.WithIamUserId(iamUserId =>
                _registrationBusinessLogic.SubmitRegistrationAsync(applicationId, iamUserId));

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

        /// <summary>
        /// Gets the company roles and roles description
        /// </summary>
        /// <param name="languageShortName" example="en">Optional two character language specifier for the roles description. Will be empty if not provided.</param>
        /// <returns>Returns the Company roles and roles description</returns>
        /// <remarks>Example: Get: /api/registration/company/companyRoles</remarks>
        /// <response code="200">Returns the Company roles data</response>
        [HttpGet]
        [Authorize(Roles = "view_company_roles")]
        [Route("company/companyRoles")]
        [ProducesResponseType(typeof(IAsyncEnumerable<CompanyRolesDetails>), StatusCodes.Status200OK)]
        public IAsyncEnumerable<CompanyRolesDetails> GetCompanyRolesAsync([FromQuery] string? languageShortName = null) =>
            _registrationBusinessLogic.GetCompanyRoles(languageShortName);

        /// <summary>
        /// Deletes the document with the given id
        /// </summary>
        /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3"></param>
        /// <returns></returns>
        /// <remarks>Example: Delete: /api/registration/documents/{documentId}</remarks>
        /// <response code="200">Successfully deleted the document</response>
        /// <response code="400">Incorrect document state</response>
        /// <response code="403">The user is not assigned with the Company Application.</response>
        /// <response code="404">The document was not found.</response>
        [HttpDelete]
        [Route("documents/{documentId}")]
        [Authorize(Roles = "delete_documents")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRegistrationDocument([FromRoute] Guid documentId)
        {
            await this.WithIamUserId(iamUserId => _registrationBusinessLogic.DeleteRegistrationDocumentAsync(documentId, iamUserId));
            return NoContent();
        }

        /// <summary>
        ///  Gets the company Identifier for Country Alpha2Code
        /// </summary>
        /// <param name="alpha2Code">Country Alpha2Code</param>
        /// <remarks>Example: Get: /api/registration/company/country/{alpha2Code}/uniqueidentifiers</remarks>
        /// <response code="200">Returns the Company Identifier data</response>
        /// <response code="404">The Unique Identifier for Country was not found.</response>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("company/country/{alpha2Code}/uniqueidentifiers")]
        [ProducesResponseType(typeof(IEnumerable<UniqueIdentifierData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public  Task<IEnumerable<UniqueIdentifierData>> GetCompanyIdentifiers([FromRoute] string alpha2Code) =>
            _registrationBusinessLogic.GetCompanyIdentifiers(alpha2Code);
    }
}
