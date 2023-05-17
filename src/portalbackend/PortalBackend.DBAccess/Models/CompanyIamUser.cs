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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
/// <summary>
/// Model for Company and Iam User
/// </summary>
public class CompanyIamUser
{
    public CompanyIamUser(Guid companyId, IEnumerable<Guid> roleIds)
    {
        CompanyId = companyId;
        RoleIds = roleIds;
    }

    /// <summary>
    /// Target IamUser Id
    /// </summary>
    /// <value></value>
    public string? TargetIamUserId { get; set; }

    /// <summary>
    /// Idp Name
    /// </summary>
    /// <value></value>
    public string? IdpName { get; set; }

    /// <summary>
    /// Id of the company
    /// </summary>
    /// <value></value>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Role Ids of User
    /// </summary>
    /// <value></value>
    public IEnumerable<Guid> RoleIds { get; set; }
}

