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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

public interface IServiceAccountCreation
{
    /// <summary>
    /// Creates the technical user account and stores the client in the service account table
    /// </summary>
    /// <param name="creationData">Creation Data</param>
    /// <param name="companyId">Id of the company the technical user is created for</param>
    /// <param name="bpns">Optional list of bpns to set for the user</param>
    /// <param name="companyServiceAccountTypeId">The type of the created service account</param>
    /// <param name="setOptionalParameter"></param>
    /// <returns>Returns information about the created technical user</returns>
    Task<(string clientId, ServiceAccountData serviceAccountData, Guid serviceAccountId, List<UserRoleData> userRoleData)> CreateServiceAccountAsync(
        ServiceAccountCreationInfo creationData,
        Guid companyId,
        IEnumerable<string> bpns,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        Action<CompanyServiceAccount>? setOptionalParameter = null);
}
