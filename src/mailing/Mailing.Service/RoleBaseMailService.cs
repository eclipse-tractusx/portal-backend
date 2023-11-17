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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Mailing.Service;

public class RoleBaseMailService : IRoleBaseMailService
{
    private readonly IPortalRepositories _portalRepositories;

    public RoleBaseMailService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    public async Task RoleBaseSendMailForCompany(IEnumerable<UserRoleConfig> receiverRoles, IEnumerable<(string ParameterName, string ParameterValue)> parameters, (string ParameterName, string ParameterValue)? userNameParameter, IEnumerable<string> template, Guid companyId)
    {
        var roleData = await GetRoleData(receiverRoles).ConfigureAwait(false);
        var companyUserWithRoleIdForCompany = _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserEmailForCompanyAndRoleId(roleData, companyId);
        await SendMailsToUsers(parameters, userNameParameter, template, companyUserWithRoleIdForCompany).ConfigureAwait(false);
    }

    public async Task RoleBaseSendMailForIdp(IEnumerable<UserRoleConfig> receiverRoles, IEnumerable<(string ParameterName, string ParameterValue)> parameters, (string ParameterName, string ParameterValue)? userNameParameter, IEnumerable<string> template, Guid identityProviderId)
    {
        var roleData = await GetRoleData(receiverRoles).ConfigureAwait(false);
        var companyUserWithRoleIdForCompany = _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyUserEmailForIdpWithoutOwnerAndRoleId(roleData, identityProviderId);
        await SendMailsToUsers(parameters, userNameParameter, template, companyUserWithRoleIdForCompany).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Guid>> GetRoleData(IEnumerable<UserRoleConfig> receiverRoles)
    {
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var roleData = await userRolesRepository
            .GetUserRoleIdsUntrackedAsync(receiverRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < receiverRoles.Sum(clientRoles => clientRoles.UserRoleNames.Count()))
        {
            throw new ConfigurationException(
                $"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", receiverRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        }

        return roleData;
    }

    private async Task SendMailsToUsers(
        IEnumerable<(string ParameterName, string ParameterValue)> parameters,
        (string ParameterName, string ParameterValue)? userNameParameter,
        IEnumerable<string> templates,
        IAsyncEnumerable<(string Email, string? FirstName, string? LastName)> companyUserWithRoleId)
    {
        await foreach (var (receiver, firstName, lastName) in companyUserWithRoleId)
        {
            IEnumerable<(string ParameterName, string ParameterValue)> ParametersWithUserName()
            {
                if (!userNameParameter.HasValue)
                {
                    return parameters;
                }

                var userName = string.Join(" ", new[] { firstName, lastName }.Where(item => !string.IsNullOrWhiteSpace(item)));
                return parameters.Append(
                    string.IsNullOrWhiteSpace(userName)
                        ? userNameParameter.Value
                        : new(userNameParameter.Value.ParameterName, userName));
            }

            var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
            var processId = processStepRepository.CreateProcess(ProcessTypeId.MAILING).Id;
            processStepRepository.CreateProcessStep(ProcessStepTypeId.SEND_MAIL, ProcessStepStatusId.TODO, processId);
            foreach (var template in templates)
            {
                _portalRepositories.GetInstance<IMailingInformationRepository>().CreateMailingInformation(processId, receiver, template, ParametersWithUserName().ToDictionary(x => x.ParameterName, x => x.ParameterValue));
            }
        }
    }
}
