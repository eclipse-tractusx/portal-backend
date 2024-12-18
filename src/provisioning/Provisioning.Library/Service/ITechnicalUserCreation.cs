/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

public interface ITechnicalUserCreation
{
    /// <summary>
    /// Creates the technical user account and stores the client in the service account table
    /// </summary>
    /// <param name="creationData">Creation Data</param>
    /// <param name="companyId">Id of the company the technical user is created for</param>
    /// <param name="bpns">Optional list of bpns to set for the user</param>
    /// <param name="technicalUserTypeId">The type of the created service account</param>
    /// <param name="enhanceTechnicalUserName">If <c>true</c> the technicalUserName will get enhanced by the id of the clientID.</param>
    /// <param name="enabled">if <c>true</c> the technical user will be enabled, otherwise <c>false</c></param>
    /// <param name="processData">The process that should be created if a role for a provider type was selected</param>
    /// <param name="setOptionalParameter"></param>
    /// <returns>Returns information about the created technical user</returns>
    Task<(bool HasExternalTechnicalUser, Guid? ProcessId, IEnumerable<CreatedServiceAccountData> TechnicalUsers)> CreateTechnicalUsersAsync(
            TechnicalUserCreationInfo creationData,
            Guid companyId,
            IEnumerable<string> bpns,
            TechnicalUserTypeId technicalUserTypeId,
            bool enhanceTechnicalUserName,
            bool enabled,
            ServiceAccountCreationProcessData? processData,
            Action<TechnicalUser>? setOptionalParameter = null);
}
