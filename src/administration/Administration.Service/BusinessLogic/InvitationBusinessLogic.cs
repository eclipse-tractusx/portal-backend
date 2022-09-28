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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Microsoft.Extensions.Options;
using PasswordGenerator;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic
{
    public class InvitationBusinessLogic : IInvitationBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalRepositories _portalRepositories;
        private readonly IMailingService _mailingService;
        private readonly InvitationSettings _settings;
        private readonly ILogger<InvitationBusinessLogic> _logger;

        public InvitationBusinessLogic(
            IProvisioningManager provisioningManager,
            IPortalRepositories portalRepositories,
            IMailingService mailingService,
            IOptions<InvitationSettings> configuration,
            ILogger<InvitationBusinessLogic> logger
            )
        {
            _provisioningManager = provisioningManager;
            _portalRepositories = portalRepositories;
            _mailingService = mailingService;
            _settings = configuration.Value;
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

            var password = new Password().Next();
            var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(
                    idpName,
                    new UserProfile(
                        string.IsNullOrWhiteSpace(invitationData.userName) ? invitationData.email : invitationData.userName,
                        invitationData.firstName,
                        invitationData.lastName,
                        invitationData.email,
                        password),
                    _provisioningManager.GetStandardAttributes(
                        alias: idpName,
                        organisationName: invitationData.organisationName
                    )
                ).ConfigureAwait(false);

            var assignedClientRoles = await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, _settings.InvitedUserInitialRoles).ConfigureAwait(false);
            var unassignedClientRoles = _settings.InvitedUserInitialRoles
                .Select(initialClientRoles => (client: initialClientRoles.Key, roles: initialClientRoles.Value.Except(assignedClientRoles[initialClientRoles.Key])))
                .Where(clientRoles => clientRoles.roles.Any())
                .ToList();
            
            var company = _portalRepositories.GetInstance<ICompanyRepository>().CreateCompany(invitationData.organisationName);
            var application = applicationRepository.CreateCompanyApplication(company, CompanyApplicationStatusId.CREATED);
            var creatorId = await userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
            var companyUser = userRepository.CreateCompanyUser(invitationData.firstName, invitationData.lastName, invitationData.email, company.Id, CompanyUserStatusId.ACTIVE, creatorId);
            foreach(var userRoleId in userRoleIds)
            {
                userRolesRepository.CreateCompanyUserAssignedRole(companyUser.Id, userRoleId);
            }
            applicationRepository.CreateInvitation(application.Id, companyUser);
            var identityProvider = identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_SHARED);
            identityProvider.Companies.Add(company);
            identityProviderRepository.CreateIamIdentityProvider(identityProvider, idpName);
            userRepository.CreateIamUser(companyUser, centralUserId);

            await _portalRepositories.SaveAsync().ConfigureAwait(false);

            if (unassignedClientRoles.Any())
            {
                throw new UnexpectedConditionException($"invalid configuration, configured roles were not assigned in keycloak: {string.Join(", ",unassignedClientRoles.Select(clientRoles => $"client: {clientRoles.client}, roles: [{string.Join(", ",clientRoles.roles)}]"))}");
            }

            var mailParameters = new Dictionary<string, string>
            {
                { "password", password },
                { "companyname", invitationData.organisationName },
                { "url", $"{_settings.RegistrationAppAddress}"},
            };

            await _mailingService.SendMails(invitationData.email, mailParameters, new List<string> { "RegistrationTemplate", "PasswordForRegistrationTemplate" }).ConfigureAwait(false);
        }
    }
}
