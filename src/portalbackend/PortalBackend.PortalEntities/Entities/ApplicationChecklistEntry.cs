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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class ApplicationChecklistEntry
{
    public ApplicationChecklistEntry(Guid applicationId, ApplicationChecklistEntryTypeId applicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId applicationChecklistEntryStatusId, DateTimeOffset dateCreated)
    {
        ApplicationId = applicationId;
        ApplicationChecklistEntryTypeId = applicationChecklistEntryTypeId;
        ApplicationChecklistEntryStatusId = applicationChecklistEntryStatusId;
        DateCreated = dateCreated;
    }

    public Guid ApplicationId { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    public ApplicationChecklistEntryTypeId ApplicationChecklistEntryTypeId { get; private set; }

    public ApplicationChecklistEntryStatusId ApplicationChecklistEntryStatusId { get; set; }

    public string? Comment { get; set; }

    // Navigation properties
    public virtual ApplicationChecklistEntryStatus? ApplicationChecklistEntryStatus { get; private set; }

    public virtual ApplicationChecklistEntryType? ApplicationChecklistEntryType { get; private set; }

    public virtual CompanyApplication? Application { get; private set; }
}
