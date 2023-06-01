/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

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

    public Task ExecuteInvitation(CompanyInvitationData invitationData, IdentityData identity)
    {
        if (string.IsNullOrWhiteSpace(invitationData.email))
        {
            throw new ControllerArgumentException("email must not be empty", "email");
        }
        if (string.IsNullOrWhiteSpace(invitationData.organisationName))
        {
            throw new ControllerArgumentException("organisationName must not be empty", "organisationName");
        }
        return ExecuteInvitationInternalAsync(invitationData, identity);
    }

    private async Task ExecuteInvitationInternalAsync(CompanyInvitationData invitationData, IdentityData identity)
    {
        var idpName = await _provisioningManager.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);
        await _provisioningManager.SetupSharedIdpAsync(idpName, invitationData.organisationName, _settings.InitialLoginTheme).ConfigureAwait(false);

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
            identity.UserId,
            idpName,
            true
        );

        IEnumerable<UserRoleData> roleDatas;
        try
        {
            roleDatas = await _userProvisioningService.GetRoleDatas(_settings.InvitedUserInitialRoles).ToListAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ConfigurationException($"{nameof(_settings.InvitedUserInitialRoles)}: {e.Message}");
        }

        var userCreationInfoIdps = new[] { new UserCreationRoleDataIdpInfo(
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
            { "companyName", invitationData.organisationName },
            { "url", $"{_settings.RegistrationAppAddress}"},
        };

        await _mailingService.SendMails(invitationData.email, mailParameters, new List<string> { "RegistrationTemplate", "PasswordForRegistrationTemplate" }).ConfigureAwait(false);
    }
}
