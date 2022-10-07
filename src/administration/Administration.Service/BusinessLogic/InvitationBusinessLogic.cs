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

using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using Microsoft.Extensions.Options;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class InvitationBusinessLogic : IInvitationBusinessLogic
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;
    private readonly InvitationSettings _settings;
    private readonly ILogger<InvitationBusinessLogic> _logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    /// <param name="mailingService">Mailing Service</param>
    /// <param name="settings">Settings</param>
    /// <param name="logger">logger</param>
    public InvitationBusinessLogic(
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        IPortalRepositories portalRepositories,
        IMailingService mailingService,
        IOptions<InvitationSettings> settings,
        ILogger<InvitationBusinessLogic> logger
        )
    {
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task ExecuteInvitation(CompanyInvitationData invitationData, string iamUserId)
    {
        if (string.IsNullOrWhiteSpace(invitationData.email))
        {
            throw new ControllerArgumentException("email must not be empty", "email");
        }
        if (string.IsNullOrWhiteSpace(invitationData.organisationName))
        {
            throw new ControllerArgumentException("organisationName must not be empty", "organisationName");
        }

        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();

        var userRoleIds = await userRolesRepository.GetUserRoleIdsUntrackedAsync(_settings.InvitedUserInitialRoles).ToListAsync().ConfigureAwait(false);
        if (userRoleIds.Count < _settings.InvitedUserInitialRoles.Sum(clientRoles => clientRoles.Value.Count()))
        {
            throw new UnexpectedConditionException($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ",_settings.InvitedUserInitialRoles.Select(clientRoles => $"client: {clientRoles.Key}, roles: [{String.Join(", ",clientRoles.Value)}]"))}");
        }

        var idpName = await _provisioningManager.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);

        await _provisioningManager.SetupSharedIdpAsync(idpName, invitationData.organisationName).ConfigureAwait(false);

        var company = _portalRepositories.GetInstance<ICompanyRepository>().CreateCompany(invitationData.organisationName);
        var application = applicationRepository.CreateCompanyApplication(company, CompanyApplicationStatusId.CREATED);
        var creatorId = await userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);

        var identityProvider = identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_SHARED);
        identityProvider.Companies.Add(company);
        identityProviderRepository.CreateIamIdentityProvider(identityProvider, idpName);

        var companyNameIdpAliasData = new CompanyNameIdpAliasData(
            company.Id,
            company.Name,
            company.BusinessPartnerNumber,
            creatorId,
            idpName,
            true
        );

        var (clientId, roles) = _settings.InvitedUserInitialRoles.Single();

        var userCreationInfoIdps = new [] { new UserCreationInfoIdp(
            invitationData.firstName,
            invitationData.lastName,
            invitationData.email,
            roles,
            string.IsNullOrWhiteSpace(invitationData.userName) ? invitationData.email : invitationData.userName,
            ""
        )}.ToAsyncEnumerable();

        var (companyUserId, userName, password, error) = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, clientId, userCreationInfoIdps).SingleAsync().ConfigureAwait(false);

        applicationRepository.CreateInvitation(application.Id, companyUserId);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        if (error != null)
        {
            throw error;
        }

        var mailParameters = new Dictionary<string, string>
        {
            { "password", password ?? "" },
            { "companyname", invitationData.organisationName },
            { "url", $"{_settings.RegistrationAppAddress}"},
        };

        await _mailingService.SendMails(invitationData.email, mailParameters, new List<string> { "RegistrationTemplate", "PasswordForRegistrationTemplate" }).ConfigureAwait(false);
    }
}
