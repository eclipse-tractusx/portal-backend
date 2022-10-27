/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class InvitationBusinessLogic : IInvitationBusinessLogic
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;
    private readonly InvitationSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    /// <param name="mailingService">Mailing Service</param>
    /// <param name="settings">Settings</param>
    public InvitationBusinessLogic(
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

    public Task ExecuteInvitation(CompanyInvitationData invitationData, string iamUserId)
    {
        if (string.IsNullOrWhiteSpace(invitationData.email))
        {
            throw new ControllerArgumentException("email must not be empty", "email");
        }
        if (string.IsNullOrWhiteSpace(invitationData.organisationName))
        {
            throw new ControllerArgumentException("organisationName must not be empty", "organisationName");
        }
        return ExecuteInvitationInternalAsync(invitationData, iamUserId);
    }

    private async Task ExecuteInvitationInternalAsync(CompanyInvitationData invitationData, string iamUserId)
    {

        var creatorId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (creatorId == Guid.Empty)
        {
            throw new ConflictException($"iamUserId {iamUserId} is not associated with a companyUser");
        }

        var idpName = await _provisioningManager.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);
        await _provisioningManager.SetupSharedIdpAsync(idpName, invitationData.organisationName).ConfigureAwait(false);

        var company = _portalRepositories.GetInstance<ICompanyRepository>().CreateCompany(invitationData.organisationName);

        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var identityProvider = identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_SHARED);
        identityProvider.Companies.Add(company);
        identityProviderRepository.CreateIamIdentityProvider(identityProvider, idpName);

        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var application = applicationRepository.CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        var companyNameIdpAliasData = new CompanyNameIdpAliasData(
            company.Id,
            company.Name,
            null,
            creatorId,
            idpName,
            true
        );

        IEnumerable<UserRoleData> roleDatas;
        try
        {
            roleDatas = await _userProvisioningService.GetRoleDatas(_settings.InvitedUserInitialRoles).ToListAsync().ConfigureAwait(false);
        }
        catch(Exception e)
        {
            throw new ConfigurationException($"{nameof(_settings.InvitedUserInitialRoles)}: {e.Message}");
        }

        var userCreationInfoIdps = new [] { new UserCreationRoleDataIdpInfo(
            invitationData.firstName,
            invitationData.lastName,
            invitationData.email,
            roleDatas,
            string.IsNullOrWhiteSpace(invitationData.userName) ? invitationData.email : invitationData.userName,
            ""
        )}.ToAsyncEnumerable();

        var (companyUserId, _, password, error) = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps).SingleAsync().ConfigureAwait(false);

        if (error != null)
        {
            throw error;
        }

        applicationRepository.CreateInvitation(application.Id, companyUserId);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        var mailParameters = new Dictionary<string, string>
        {
            { "password", password ?? "" },
            { "companyname", invitationData.organisationName },
            { "url", $"{_settings.RegistrationAppAddress}"},
        };

        await _mailingService.SendMails(invitationData.email, mailParameters, new List<string> { "RegistrationTemplate", "PasswordForRegistrationTemplate" }).ConfigureAwait(false);
    }
}
