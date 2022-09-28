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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Auditing;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditCompanyUserAssignedRoleCplp1440DbAuditing))]
public class CompanyUserAssignedRole : IAuditable
{
    protected CompanyUserAssignedRole() {}

    public CompanyUserAssignedRole(Guid companyUserId, Guid userRoleId)
    {
        CompanyUserId = companyUserId;
        UserRoleId = userRoleId;
    }
    
    public Guid Id { get; set; }
    public Guid CompanyUserId { get; private set; }
    public Guid UserRoleId { get; private set; }
    
    /// <inheritdoc />
    public Guid? LastEditorId { get; set; }
    // Navigation properties
    public virtual CompanyUser? CompanyUser { get; private set; }
    public virtual UserRole? UserRole { get; private set; }
}
