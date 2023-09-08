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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Service;

public class RoleBaseMailService : IRoleBaseMailService
{

    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;

    public RoleBaseMailService(IPortalRepositories portalRepositories, IMailingService mailingService)
    {
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
    }
    public async Task RoleBaseSendMail(IEnumerable<UserRoleConfig> receiverRoles, IEnumerable<(string ParameterName, string ParameterValue)> parameters, (string ParameterName, string ParameterValue)? userNameParameter, IEnumerable<string> template, Guid companyId)
    {
        var receiverUserRoles = receiverRoles;
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var roleData = await userRolesRepository
            .GetUserRoleIdsUntrackedAsync(receiverUserRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < receiverUserRoles.Sum(clientRoles => clientRoles.UserRoleNames.Count()))
        {
            throw new ConfigurationException(
                $"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", receiverUserRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        }

        var companyUserWithRoleIdForCompany = _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserEmailForCompanyAndRoleId(roleData, companyId);

        await foreach (var (receiver, firstName, lastName) in companyUserWithRoleIdForCompany)
        {
            IEnumerable<(string ParameterName, string ParameterValue)> ParametersWithUserName()
            {
                if (userNameParameter.HasValue)
                {
                    var userName = string.Join(" ", new[] { firstName, lastName }.Where(item => !string.IsNullOrWhiteSpace(item)));
                    return parameters.Append(
                        string.IsNullOrWhiteSpace(userName)
                            ? userNameParameter.Value
                            : new(userNameParameter.Value.ParameterName, userName));
                }
                return parameters;
            }

            await _mailingService.SendMails(receiver, ParametersWithUserName().ToImmutableDictionary(x => x.ParameterName, x => x.ParameterValue), template).ConfigureAwait(false);
        }
    }
}
