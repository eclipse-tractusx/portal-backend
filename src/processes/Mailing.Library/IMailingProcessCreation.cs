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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;

public interface IMailingProcessCreation
{
    void CreateMailProcess(string email, string template, Dictionary<string, string> mailParameters);
    Task RoleBaseSendMail(IEnumerable<UserRoleConfig> receiverRoles, IEnumerable<(string ParameterName, string ParameterValue)> parameters, (string ParameterName, string ParameterValue)? userNameParameter, IEnumerable<string> templates, Guid companyId);
    Task RoleBaseSendMailForIdp(IEnumerable<UserRoleConfig> receiverRoles, IEnumerable<(string ParameterName, string ParameterValue)> parameters, (string ParameterName, string ParameterValue)? userNameParameter, IEnumerable<string> templates, Guid identityProviderId);
    Task<List<Guid>> GetRoleData(IEnumerable<UserRoleConfig> receiverRoles);
}
