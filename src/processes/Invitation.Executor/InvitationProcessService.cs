/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Security.Cryptography;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor;

public class InvitationProcessService : IInvitationProcessService
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IMailingService _mailingService;
    private readonly InvitationSettings _settings;
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    /// <param name="mailingService">Mailing Service</param>
    /// <param name="settings">Settings</param>
    public InvitationProcessService(
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        IPortalRepositories portalRepositories,
        IMailingService mailingService,
        IOptions<InvitationSettings> settings)
    {
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
        _settings = settings.Value;
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SetupIdp(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var orgName = await companyInvitationRepository.GetOrganisationNameForInvitation(invitationId).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(orgName))
        {
            throw new ConflictException("Org name must not be null");
        }
        var idpName = await _provisioningManager.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);
        await _provisioningManager.SetupSharedIdpAsync(idpName, orgName, _settings.InitialLoginTheme).ConfigureAwait(false);

        companyInvitationRepository.AttachAndModifyCompanyInvitation(invitationId, x => { x.IdpName = null; }, x => { x.IdpName = idpName; });

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateIdpDatabase(Guid companyInvitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var (exists, orgName, idpName) = await companyInvitationRepository.GetInvitationIdpCreationData(companyInvitationId).ConfigureAwait(false);
        if (!exists)
        {
            throw new NotFoundException($"CompanyInvitation {companyInvitationId} does not exist");
        }

        if (string.IsNullOrWhiteSpace(idpName))
        {
            throw new ConflictException("IdpName must be set for the company invitation");
        }

        var company = _portalRepositories.GetInstance<ICompanyRepository>().CreateCompany(orgName);

        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var identityProvider = identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, company.Id, null);
        identityProvider.Companies.Add(company);
        identityProviderRepository.CreateIamIdentityProvider(identityProvider.Id, idpName);

        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var applicationId = applicationRepository.CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL).Id;
        companyInvitationRepository.AttachAndModifyCompanyInvitation(companyInvitationId, x => { x.ApplicationId = null; }, x => { x.ApplicationId = applicationId; });

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_USER, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateUser(Guid companyInvitationId, CancellationToken cancellationToken)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var (exists, applicationId, companyId, companyName, idpInformation, userInformation) = await companyInvitationRepository.GetInvitationUserData(companyInvitationId).ConfigureAwait(false);

        if (!exists)
        {
            throw new NotFoundException($"CompanyInvitation {companyInvitationId} does not exist");
        }

        if (applicationId == null)
        {
            throw new ConflictException("Application must be set for the company invitation");
        }

        if (companyId == null)
        {
            throw new ConflictException("Company must be set for the company invitation");
        }

        if (idpInformation.Count() != 1)
        {
            throw new ConflictException("There must only exist one idp for the company invitation");
        }

        var (idpId, idpName) = idpInformation.Single();
        var companyNameIdpAliasData = new CompanyNameIdpAliasData(
            companyId.Value,
            companyName,
            null,
            idpName,
            idpId,
            true
        );

        IEnumerable<UserRoleData> roleDatas;
        try
        {
            roleDatas = await _userProvisioningService.GetRoleDatas(_settings.InvitedUserInitialRoles).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ConfigurationException($"{nameof(_settings.InvitedUserInitialRoles)}: {e.Message}");
        }

        var userCreationInfoIdps = new[] { new UserCreationRoleDataIdpInfo(
            userInformation.FirstName,
            userInformation.LastName,
            userInformation.Email,
            roleDatas,
            string.IsNullOrWhiteSpace(userInformation.UserName) ? userInformation.Email : userInformation.UserName,
            "",
            UserStatusId.ACTIVE,
            true
        )}.ToAsyncEnumerable();

        var (companyUserId, _, password, error) = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps, cancellationToken).SingleAsync(cancellationToken).ConfigureAwait(false);

        if (error is not null)
        {
            throw error;
        }
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_settings.EncryptionKey);
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using (var memoryStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                using var sw = new StreamWriter(cryptoStream, Encoding.UTF8);
                sw.Write(password);
            }
            var secret = memoryStream.ToArray();
            companyInvitationRepository.AttachAndModifyCompanyInvitation(companyInvitationId, x =>
                {
                    x.Password = null;
                },
                x =>
                {
                    x.Password = secret;
                });
        }
        _portalRepositories.GetInstance<IApplicationRepository>().CreateInvitation(applicationId.Value, companyUserId);

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_SEND_MAIL, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendMail(Guid companyInvitationId)
    {
        var (exists, orgName, userPassword, email) = await _portalRepositories.GetInstance<ICompanyInvitationRepository>().GetMailData(companyInvitationId).ConfigureAwait(false);

        if (!exists)
        {
            throw new NotFoundException($"CompanyInvitation {companyInvitationId} does not exist");
        }

        if (userPassword is null)
        {
            throw new ConflictException("Password needs to be set");
        }
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_settings.EncryptionKey);
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using (var msDecrypt = new MemoryStream(userPassword))
        {
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);
            var password = srDecrypt.ReadToEnd();
            var mailParameters = new Dictionary<string, string>
            {
                { "password", password ?? "" },
                { "companyName", orgName },
                { "url", _settings.RegistrationAppAddress },
                { "passwordResendUrl", _settings.PasswordResendAddress },
            };

            await _mailingService.SendMails(email, mailParameters, new List<string> { "RegistrationTemplate", "PasswordForRegistrationTemplate" }).ConfigureAwait(false);
        }

        return (null, ProcessStepStatusId.DONE, true, null);
    }
}
