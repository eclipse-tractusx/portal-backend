﻿/********************************************************************************
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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Auditing;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class CompanyApplication : IAuditable
{
    protected CompanyApplication()
    {
        Invitations = new HashSet<Invitation>();
    }

    public CompanyApplication(Guid id, Guid companyId, CompanyApplicationStatusId applicationStatusId, DateTimeOffset dateCreated) : this()
    {
        Id = id;
        CompanyId = companyId;
        ApplicationStatusId = applicationStatusId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; private set; }

    public DateTimeOffset? DateLastChanged { get; set; }

    public CompanyApplicationStatusId ApplicationStatusId { get; set; }
    public Guid CompanyId { get; private set; }

    /// <inheritdoc />
    public Guid? LastEditorId { get; set; }
    // Navigation properties
    public virtual CompanyApplicationStatus? ApplicationStatus { get; set; }
    public virtual Company? Company { get;  set; }
    public virtual ICollection<Invitation> Invitations { get; private set; }
}
