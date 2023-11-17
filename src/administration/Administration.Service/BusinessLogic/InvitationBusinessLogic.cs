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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class InvitationBusinessLogic : IInvitationBusinessLogic
{
    private static readonly Regex Company = new(ValidationExpressions.Company, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly InvitationSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    /// <param name="settings">Settings</param>
    public InvitationBusinessLogic(
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        IPortalRepositories portalRepositories,
        IOptions<InvitationSettings> settings)
    {
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _portalRepositories = portalRepositories;
        _settings = settings.Value;
    }

    public Task ExecuteInvitation(CompanyInvitationData invitationData)
    {
        if (string.IsNullOrWhiteSpace(invitationData.email))
        {
            throw new ControllerArgumentException("email must not be empty", "email");
        }

        if (string.IsNullOrWhiteSpace(invitationData.organisationName))
        {
            throw new ControllerArgumentException("organisationName must not be empty", "organisationName");
        }

        if (!string.IsNullOrEmpty(invitationData.organisationName) && !Company.IsMatch(invitationData.organisationName))
        {
            throw new ControllerArgumentException("OrganisationName length must be 3-40 characters and *+=#%\\s not used as one of the first three characters in the Organisation name", "organisationName");
        }

        return ExecuteInvitationInternalAsync(invitationData);
    }

    private async Task ExecuteInvitationInternalAsync(CompanyInvitationData invitationData)
    {
        var idpName = await _provisioningManager.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);
        await _provisioningManager.SetupSharedIdpAsync(idpName, invitationData.organisationName, _settings.InitialLoginTheme).ConfigureAwait(false);

        var company = _portalRepositories.GetInstance<ICompanyRepository>().CreateCompany(invitationData.organisationName);

        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var identityProvider = identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, company.Id, null);
        identityProvider.Companies.Add(company);
        identityProviderRepository.CreateIamIdentityProvider(identityProvider.Id, idpName);

        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var application = applicationRepository.CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL);

        var companyNameIdpAliasData = new CompanyNameIdpAliasData(
            company.Id,
            company.Name,
            null,
            idpName,
            identityProvider.Id,
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
            "",
            UserStatusId.ACTIVE,
            true
        )}.ToAsyncEnumerable();

        var (companyUserId, _, password, error) = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps).SingleAsync().ConfigureAwait(false);

        if (error != null)
        {
            throw error;
        }

        applicationRepository.CreateInvitation(application.Id, companyUserId);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.MAILING).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.SEND_MAIL, ProcessStepStatusId.TODO, processId);
        var mailParameters = new Dictionary<string, string>
        {
            { "password", password ?? "" },
            { "companyName", invitationData.organisationName },
            { "url", _settings.RegistrationAppAddress },
            { "passwordResendUrl", _settings.PasswordResendAddress },
            { "closeApplicationUrl", _settings.CloseApplicationAddress },
        };

        _portalRepositories.GetInstance<IMailingInformationRepository>().CreateMailingInformation(processId, invitationData.email, "RegistrationTemplate", mailParameters);
        _portalRepositories.GetInstance<IMailingInformationRepository>().CreateMailingInformation(processId, invitationData.email, "PasswordForRegistrationTemplate", mailParameters);
    }
}
