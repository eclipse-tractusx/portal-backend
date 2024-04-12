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
using Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor;

public class InvitationProcessService : IInvitationProcessService
{
    private const string IdpNotSetErrorMessage = "Idp name must not be null";

    private readonly IIdpManagement _idpManagement;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly InvitationSettings _settings;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingProcessCreation _mailingProcessCreation;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="idpManagement">Shared Idp Creation</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    /// <param name="mailingProcessCreation">MailingProcessCreation</param>
    /// <param name="settings">Settings</param>
    public InvitationProcessService(
        IIdpManagement idpManagement,
        IUserProvisioningService userProvisioningService,
        IPortalRepositories portalRepositories,
        IMailingProcessCreation mailingProcessCreation,
        IOptions<InvitationSettings> settings)
    {
        _idpManagement = idpManagement;
        _userProvisioningService = userProvisioningService;
        _portalRepositories = portalRepositories;
        _mailingProcessCreation = mailingProcessCreation;
        _settings = settings.Value;
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateCentralIdp(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var orgName = await companyInvitationRepository.GetOrganisationNameForInvitation(invitationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrWhiteSpace(orgName))
        {
            throw new ConflictException("Org name must not be null");
        }

        var idpName = await _idpManagement.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);
        await _idpManagement.CreateCentralIdentityProviderAsync(idpName, orgName).ConfigureAwait(ConfigureAwaitOptions.None);
        companyInvitationRepository.AttachAndModifyCompanyInvitation(invitationId, x => { x.IdpName = null; }, x => { x.IdpName = idpName; });

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateSharedIdpServiceAccount(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var idpName = await companyInvitationRepository.GetIdpNameForInvitationId(invitationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrWhiteSpace(idpName))
        {
            throw new ConflictException(IdpNotSetErrorMessage);
        }

        var (clientId, clientSecret, serviceAccountUserId) = await _idpManagement.CreateSharedIdpServiceAccountAsync(idpName).ConfigureAwait(ConfigureAwaitOptions.None);

        var (secret, initializationVector, encryptionMode) = Encrypt(clientSecret);

        companyInvitationRepository.AttachAndModifyCompanyInvitation(invitationId, x =>
            {
                x.ClientId = null;
                x.ClientSecret = null;
                x.ServiceAccountUserId = null;
            },
            x =>
            {
                x.ClientId = clientId;
                x.ClientSecret = secret;
                x.InitializationVector = initializationVector;
                x.EncryptionMode = encryptionMode;
                x.ServiceAccountUserId = serviceAccountUserId;
            });

        companyInvitationRepository.AttachAndModifyCompanyInvitation(invitationId, x => { x.IdpName = null; }, x => { x.IdpName = idpName; });

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_ADD_REALM_ROLE, 1), ProcessStepStatusId.DONE, true, null);
    }

    private (byte[] Secret, byte[] InitializationVector, int EncryptionMode) Encrypt(string clientSecret)
    {
        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == _settings.EncryptionConfigIndex) ?? throw new ConfigurationException($"EncryptionModeIndex {_settings.EncryptionConfigIndex} is not configured");
        var (secret, initializationVector) = CryptoHelper.Encrypt(clientSecret, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
        return (secret, initializationVector, _settings.EncryptionConfigIndex);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> AddRealmRoleMappingsToUserAsync(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var serviceAccountUserId = await companyInvitationRepository.GetServiceAccountUserIdForInvitation(invitationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrWhiteSpace(serviceAccountUserId))
        {
            throw new ConflictException("ServiceAccountUserId must not be null");
        }

        await _idpManagement.AddRealmRoleMappingsToUserAsync(serviceAccountUserId).ConfigureAwait(ConfigureAwaitOptions.None);

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> UpdateCentralIdpUrl(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var (orgName, idpName, clientId, clientSecret, initializationVector, encryptionMode) = await companyInvitationRepository.GetUpdateCentralIdpUrlData(invitationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrWhiteSpace(idpName))
        {
            throw new ConflictException(IdpNotSetErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ConflictException("ClientId must not be null");
        }

        var secret = Decrypt(clientSecret, initializationVector, encryptionMode);

        await _idpManagement.UpdateCentralIdentityProviderUrlsAsync(idpName, orgName, _settings.InitialLoginTheme, clientId, secret).ConfigureAwait(false);

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER, 1), ProcessStepStatusId.DONE, true, null);
    }

    private string Decrypt(byte[]? clientSecret, byte[]? initializationVector, int? encryptionMode)
    {
        if (clientSecret == null)
        {
            throw new ConflictException("ClientSecret must not be null");
        }

        if (encryptionMode == null)
        {
            throw new ConflictException("EncryptionMode must not be null");
        }

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == encryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {encryptionMode} is not configured");

        return CryptoHelper.Decrypt(clientSecret, initializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateCentralIdpOrgMapper(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var (exists, orgName, idpName) = await companyInvitationRepository.GetIdpAndOrgName(invitationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!exists)
        {
            throw new ConflictException($"Invitation {invitationId} does not exist");
        }
        if (string.IsNullOrWhiteSpace(idpName))
        {
            throw new ConflictException(IdpNotSetErrorMessage);
        }

        await _idpManagement.CreateCentralIdentityProviderOrganisationMapperAsync(idpName, orgName).ConfigureAwait(ConfigureAwaitOptions.None);

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateSharedIdpRealm(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var (orgName, idpName, clientId, clientSecret, initializationVector, encryptionMode) = await companyInvitationRepository.GetUpdateCentralIdpUrlData(invitationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrWhiteSpace(idpName))
        {
            throw new ConflictException(IdpNotSetErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ConflictException("ClientId must not be null");
        }

        var secret = Decrypt(clientSecret, initializationVector, encryptionMode);

        await _idpManagement
            .CreateSharedRealmIdpClientAsync(idpName, _settings.InitialLoginTheme, orgName, clientId, secret)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_SHARED_CLIENT, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateSharedClient(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var (_, idpName, clientId, clientSecret, initializationVector, encryptionMode) = await companyInvitationRepository.GetUpdateCentralIdpUrlData(invitationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrWhiteSpace(idpName))
        {
            throw new ConflictException(IdpNotSetErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ConflictException("ClientId must not be null");
        }

        var secret = Decrypt(clientSecret, initializationVector, encryptionMode);

        await _idpManagement
            .CreateSharedClientAsync(idpName, clientId, secret)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> EnableCentralIdp(Guid invitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var idpName = await companyInvitationRepository.GetIdpNameForInvitationId(invitationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrWhiteSpace(idpName))
        {
            throw new ConflictException(IdpNotSetErrorMessage);
        }

        await _idpManagement
            .EnableCentralIdentityProviderAsync(idpName)
            .ConfigureAwait(false);

        return (Enumerable.Repeat(ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP, 1), ProcessStepStatusId.DONE, true, null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateIdpDatabase(Guid companyInvitationId)
    {
        var companyInvitationRepository = _portalRepositories.GetInstance<ICompanyInvitationRepository>();
        var (exists, orgName, idpName) = await companyInvitationRepository.GetIdpAndOrgName(companyInvitationId).ConfigureAwait(ConfigureAwaitOptions.None);
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
        var (exists, applicationId, companyId, companyName, idpInformation, userInformation) = await companyInvitationRepository.GetInvitationUserData(companyInvitationId).ConfigureAwait(ConfigureAwaitOptions.None);

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

        foreach (var template in new[] { "RegistrationTemplate", "PasswordForRegistrationTemplate" })
        {
            var mailParameters = ImmutableDictionary.CreateRange(new[]
            {
                KeyValuePair.Create("password", password ?? ""),
                KeyValuePair.Create("companyName", companyName),
                KeyValuePair.Create("url", _settings.RegistrationAppAddress),
                KeyValuePair.Create("passwordResendUrl", _settings.PasswordResendAddress),
            });
            _mailingProcessCreation.CreateMailProcess(userInformation.Email, template, mailParameters);
        }

        _portalRepositories.GetInstance<IApplicationRepository>().CreateInvitation(applicationId.Value, companyUserId);

        return (null, ProcessStepStatusId.DONE, true, null);
    }
}
