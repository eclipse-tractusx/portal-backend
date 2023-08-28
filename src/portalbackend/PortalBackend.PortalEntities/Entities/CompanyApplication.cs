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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditCompanyApplication20230824))]
public class CompanyApplication : IAuditableV1, IBaseEntity
{
    protected CompanyApplication()
    {
        Invitations = new HashSet<Invitation>();
        ApplicationChecklistEntries = new HashSet<ApplicationChecklistEntry>();
    }

    public CompanyApplication(Guid id, Guid companyId, CompanyApplicationStatusId applicationStatusId, CompanyApplicationTypeId companyApplicationTypeId, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        CompanyId = companyId;
        ApplicationStatusId = applicationStatusId;
        CompanyApplicationTypeId = companyApplicationTypeId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; private set; }

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }

    public CompanyApplicationStatusId ApplicationStatusId { get; set; }
    public Guid CompanyId { get; private set; }
    public Guid? ChecklistProcessId { get; set; }

    public CompanyApplicationTypeId CompanyApplicationTypeId { get; private set; }

    public Guid? OnboardingServiceProviderId { get; private set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // Navigation properties
    public virtual CompanyApplicationStatus? ApplicationStatus { get; set; }
    public virtual Company? Company { get; set; }
    public virtual Process? ChecklistProcess { get; set; }
    public virtual CompanyApplicationType? CompanyApplicationType { get; set; }
    public virtual Company? OnboardingServiceProvider { get; set; }
    public virtual ICollection<Invitation> Invitations { get; private set; }
    public virtual ICollection<ApplicationChecklistEntry> ApplicationChecklistEntries { get; private set; }
    public virtual Identity? LastEditor { get; private set; }
}
