using CatenaX.NetworkServices.Consent.Library.Data;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Registration.Service.BusinessLogic;
using CatenaX.NetworkServices.Registration.Service.CustomException;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Registration.Service.Controllers
{
    [ApiController]
    [Route("api/registration")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILogger<RegistrationController> _logger;
        private readonly IRegistrationBusinessLogic _registrationBusinessLogic;

        public RegistrationController(ILogger<RegistrationController> logger, IRegistrationBusinessLogic registrationBusinessLogic)
        {
            _logger = logger;
            _registrationBusinessLogic = registrationBusinessLogic;
        }

        [HttpGet]
        [Authorize(Roles = "add_company_data")]
        [Route("company/{bpn}")]
        [ProducesResponseType(typeof(Company), (int)HttpStatusCode.OK)]
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

        [HttpPost]
        [Authorize(Policy = "CheckTenant")]
        [Authorize(Roles = "invite_user")]
        [Route("tenant/{tenant}/users")]
        public async Task<IActionResult> CreateUsersAsync([FromRoute] string tenant, [FromBody] List<UserCreationInfo> usersToCreate)
        {
            try
            {
                var createdByName = User.Claims.SingleOrDefault(x => x.Type == "name").Value as string;
                var createdUsers = await _registrationBusinessLogic.CreateUsersAsync(usersToCreate, tenant, createdByName).ConfigureAwait(false);

                return Ok(createdUsers);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [Authorize(Roles = "submit_registration")]
        [Route("custodianWallet")]
        public async Task<IActionResult> CreateWallet([FromBody] WalletInformation walletToCreate)
        {
            try
            {
                await _registrationBusinessLogic.CreateCustodianWalletAsync(walletToCreate).ConfigureAwait(false);
                return Ok();
            }
            catch (ServiceException e)
            {
                var content = new { message = e.Message };
                return new ContentResult { StatusCode = (int)e.StatusCode, Content = JsonConvert.SerializeObject(content), ContentType = "application/json" };
            }
        }

        [HttpPost]
        [Authorize(Roles = "upload_documents")]
        [Route("application/{applicationId}/documentType/{documentTypeId}/documents")]
        public async Task<IActionResult> UploadDocumentAsync([FromRoute] Guid applicationId,[FromRoute] DocumentTypeId documentTypeId,[FromForm(Name = "document")] IFormFile document)
        {
            try
            {
                if (string.IsNullOrEmpty(document.FileName))
                {
                    return BadRequest();
                }
                WithIamUserId(user => _registrationBusinessLogic.UploadDocumentAsync(applicationId,document,user,documentTypeId));
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        [Authorize(Roles = "invite_user")]
        [Route("companyRoles")]
        public async Task<IActionResult> SetCompanyRolesAsync([FromBody] CompanyToRoles rolesToSet)
        {
            await _registrationBusinessLogic.SetCompanyRolesAsync(rolesToSet).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("companyRoles")]
        [ProducesResponseType(typeof(List<CompanyRole>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCompanyRolesAsync() =>
            Ok((await _registrationBusinessLogic.GetCompanyRolesAsync().ConfigureAwait(false)).ToList());

        [HttpGet]
        [Authorize(Roles = "sign_consent")]
        [Route("consentsForCompanyRole/{roleId}")]
        [ProducesResponseType(typeof(List<ConsentForCompanyRole>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCompanyRolesAsync(int roleId) =>
            Ok((await _registrationBusinessLogic.GetConsentForCompanyRoleAsync(roleId).ConfigureAwait(false)).ToList());

        [HttpGet]
        [Authorize(Roles = "sign_consent")]
        [Route("signedConsentsByCompanyId/{companyId}")]
        [ProducesResponseType(typeof(List<SignedConsent>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SignedConsentsByCompanyIdAsync(string companyId) =>
            Ok((await _registrationBusinessLogic.SignedConsentsByCompanyIdAsync(companyId).ConfigureAwait(false)).ToList());

        [HttpPut]
        [Authorize(Roles = "sign_consent")]
        [Route("signConsent")]
        public async Task<IActionResult> SignConsentAsync([FromBody] SignConsentRequest signConsentRequest)
        {
            await _registrationBusinessLogic.SignConsentAsync(signConsentRequest).ConfigureAwait(false);
            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = "invite_user")]
        [Route("idp")]
        public async Task<IActionResult> SetIdpAsync([FromBody] SetIdp idpToSet)
        {
            await _registrationBusinessLogic.SetIdpAsync(idpToSet).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("rolesComposite")]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
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

        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("applications")]
        public IAsyncEnumerable<CompanyApplication> GetApplicationsWithStatusAsync() =>
            WithIamUserId(user => _registrationBusinessLogic.GetAllApplicationsForUserWithStatus(user));

        [HttpPut]
        [Authorize(Roles = "submit_registration")]
        [Route("application/{applicationId}/status")]
        public Task<int> SetApplicationStatusAsync([FromRoute] Guid applicationId, [FromQuery] CompanyApplicationStatusId status) =>
            _registrationBusinessLogic.SetApplicationStatusAsync(applicationId, status);

        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("application/{applicationId}/status")]
        public Task<CompanyApplicationStatusId> GetApplicationStatusAsync([FromRoute] Guid applicationId) =>
            _registrationBusinessLogic.GetApplicationStatusAsync(applicationId);

        [HttpGet]
        [Authorize(Roles = "view_registration")]
        [Route("application/{applicationId}/companyDetailsWithAddress")]
        public Task<CompanyWithAddress> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
            _registrationBusinessLogic.GetCompanyWithAddressAsync(applicationId);

        [HttpPost]
        [Authorize(Roles = "add_company_data")]
        [Route("application/{applicationId}/companyDetailsWithAddress")]
        public Task SetCompanyWithAddressAsync([FromRoute] Guid applicationId, [FromBody] CompanyWithAddress companyWithAddress) => 
            _registrationBusinessLogic.SetCompanyWithAddressAsync(applicationId, companyWithAddress);

        [HttpPost]
        [Authorize(Roles = "invite_user")]
        [Route("application/{applicationId}/inviteNewUser")]
        public Task<int> InviteNewUserAsync([FromRoute] Guid applicationId, [FromBody] UserInvitationData userInvitationData) =>
            _registrationBusinessLogic.InviteNewUserAsync(applicationId, userInvitationData);

        [HttpPut]
        [Authorize(Roles = "submit_registration")]
        [Route("application/{applicationId}/agreement/{agreementId}/confirmConsent")]
        public Task<int> ConfirmConsentToAgreementAsync([FromRoute] Guid applicationId, [FromRoute] Guid agreementId, [FromQuery] int companyRoleId) =>
            WithIamUserId(iamUserId =>
                _registrationBusinessLogic.SubmitRoleConsentAsync(applicationId, agreementId, companyRoleId, iamUserId));

        [HttpPost]
        [Authorize(Roles = "submit_registration")]
        [Route("submitregistration")]
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

        private T WithIamUserId<T>(Func<string,T> _next) =>
            _next(User.Claims.SingleOrDefault(x => x.Type == "sub").Value as string);
    }
}
