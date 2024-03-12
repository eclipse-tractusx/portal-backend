/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;

public class MailingProcessCreation : IMailingProcessCreation
{
    private readonly IPortalRepositories _portalRepositories;

    public MailingProcessCreation(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    public void CreateMailProcess(string email, string template, IReadOnlyDictionary<string, string> mailParameters)
    {
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.MAILING).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.SEND_MAIL, ProcessStepStatusId.TODO, processId);
        _portalRepositories.GetInstance<IMailingInformationRepository>().CreateMailingInformation(processId, email, template, mailParameters);
    }

    public async Task RoleBaseSendMail(IEnumerable<UserRoleConfig> receiverRoles, IEnumerable<(string ParameterName, string ParameterValue)> parameters, (string ParameterName, string ParameterValue)? userNameParameter, IEnumerable<string> templates, Guid companyId)
    {
        var roleData = await GetRoleData(receiverRoles);
        var companyUserWithRoleIdForCompany = _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserEmailForCompanyAndRoleId(roleData, companyId);
        await CreateMailProcesses(parameters, userNameParameter, templates, companyUserWithRoleIdForCompany);
    }

    public async Task RoleBaseSendMailForIdp(IEnumerable<UserRoleConfig> receiverRoles,
        IEnumerable<(string ParameterName, string ParameterValue)> parameters,
        (string ParameterName, string ParameterValue)? userNameParameter, IEnumerable<string> templates,
        Guid identityProviderId)
    {
        var roleData = await GetRoleData(receiverRoles);
        var companyUserWithRoleIdForIdp = _portalRepositories.GetInstance<IIdentityProviderRepository>()
            .GetCompanyUserEmailForIdpWithoutOwnerAndRoleId(roleData, identityProviderId);
        await CreateMailProcesses(parameters, userNameParameter, templates, companyUserWithRoleIdForIdp);
    }

    public async Task<IEnumerable<Guid>> GetRoleData(IEnumerable<UserRoleConfig> receiverRoles)
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

        return roleData;
    }

    private async Task CreateMailProcesses(IEnumerable<(string ParameterName, string ParameterValue)> parameters,
        (string ParameterName, string ParameterValue)? userNameParameter, IEnumerable<string> templates,
        IAsyncEnumerable<(string Email, string? FirstName, string? LastName)> companyUserWithRoleIdForCompany)
    {
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
