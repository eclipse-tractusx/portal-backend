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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library;

public class NetworkRegistrationHandler : INetworkRegistrationHandler
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly NetworkRegistrationProcessSettings _settings;
    private readonly IMailingService _mailingService;

    public NetworkRegistrationHandler(
        IPortalRepositories portalRepositories,
        IUserProvisioningService userProvisioningService,
        IProvisioningManager provisioningManager,
        IMailingService mailingService,
        IOptions<NetworkRegistrationProcessSettings> options)
    {
        _portalRepositories = portalRepositories;
        _userProvisioningService = userProvisioningService;
        _provisioningManager = provisioningManager;
        _mailingService = mailingService;

        _settings = options.Value;
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SynchronizeUser(Guid networkRegistrationId)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userRoleRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var ospName = await _portalRepositories.GetInstance<INetworkRepository>().GetOspCompanyName(networkRegistrationId).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(ospName))
        {
            throw new UnexpectedConditionException("Onboarding Service Provider name must be set");
        }

        var companyAssignedIdentityProviders = await userRepository
            .GetUserAssignedIdentityProviderForNetworkRegistration(networkRegistrationId)
            .ToListAsync()
            .ConfigureAwait(false);
        var roleData = await _userProvisioningService.GetRoleDatas(_settings.InitialRoles).ToListAsync().ConfigureAwait(false);

        foreach (var cu in companyAssignedIdentityProviders)
        {
            if (string.IsNullOrWhiteSpace(cu.FirstName) || string.IsNullOrWhiteSpace(cu.LastName) ||
                string.IsNullOrWhiteSpace(cu.Email))
            {
                throw new ConflictException(
                    $"Firstname, Lastname & Email of CompanyUser {cu.CompanyUserId} must not be null here");
            }

            if (cu.ProviderLinkData.Any(x => string.IsNullOrWhiteSpace(x.Alias)))
            {
                throw new ConflictException($"Alias must be set for all ProviderLinkData of CompanyUser {cu.CompanyUserId}");
            }

            try
            {
                var userId = await _provisioningManager.GetUserByUserName(cu.CompanyUserId.ToString()).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    userRepository.AttachAndModifyIdentity(cu.CompanyUserId, i =>
                        {
                            i.UserStatusId = UserStatusId.PENDING;
                            i.UserEntityId = null;
                        },
                        i =>
                        {
                            i.UserStatusId = UserStatusId.ACTIVE;
                            i.UserEntityId = userId;
                        });

                    await _userProvisioningService.AssignRolesToNewUserAsync(userRoleRepository, roleData, (userId, cu.CompanyUserId)).ConfigureAwait(false);
                    continue;
                }

                await _userProvisioningService.HandleCentralKeycloakCreation(new UserCreationRoleDataIdpInfo(cu.FirstName!, cu.LastName!, cu.Email!, roleData, string.Empty, string.Empty, UserStatusId.ACTIVE, true), cu.CompanyUserId, cu.CompanyName, cu.Bpn, null, cu.ProviderLinkData.Select(x => new IdentityProviderLink(x.Alias!, x.ProviderUserId, x.UserName)), userRepository, userRoleRepository).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new ServiceException(e.Message, true);
            }
        }

        await SendMails(companyAssignedIdentityProviders.Select(x => new UserMailInformation(x.Email!, x.FirstName, x.LastName, x.ProviderLinkData.Select(pld => pld.Alias!))), ospName).ConfigureAwait(false);
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    private async Task SendMails(IEnumerable<UserMailInformation> companyUserWithRoleIdForCompany, string ospName)
    {
        foreach (var (receiver, firstName, lastName, idpAliasse) in companyUserWithRoleIdForCompany)
        {
            var userName = string.Join(" ", firstName, lastName);
            var mailParameters = new Dictionary<string, string>
            {
                { "userName", !string.IsNullOrWhiteSpace(userName) ? userName : receiver },
                { "hostname", _settings.BasePortalAddress },
                { "osp", ospName },
                { "url", _settings.BasePortalAddress },
                { "idpAliasse", string.Join(",", idpAliasse) }
            };
            await _mailingService.SendMails(receiver, mailParameters, Enumerable.Repeat("OspWelcomeMail", 1)).ConfigureAwait(false);
        }
    }
}
