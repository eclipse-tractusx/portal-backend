/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class NetworkRegistration : IBaseEntity
{
    private NetworkRegistration()
    {
        ExternalId = null!;
    }

    public NetworkRegistration(Guid id, string externalId, Guid companyId, Guid processId, Guid onboardingServiceProviderId, Guid applicationId, DateTimeOffset dateCreated)
        : this()
    {
        Id = id;
        ExternalId = externalId;
        CompanyId = companyId;
        ProcessId = processId;
        OnboardingServiceProviderId = onboardingServiceProviderId;
        ApplicationId = applicationId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; }

    public DateTimeOffset DateCreated { get; set; }

    public string ExternalId { get; set; }

    public Guid CompanyId { get; set; }

    public Guid OnboardingServiceProviderId { get; set; }

    public Guid ApplicationId { get; set; }

    public Guid ProcessId { get; set; }

    public virtual Company? Company { get; private set; }

    public virtual Company? OnboardingServiceProvider { get; private set; }

    public virtual CompanyApplication? CompanyApplication { get; private set; }

    public virtual Process<ProcessTypeId, ProcessStepTypeId>? Process { get; private set; }
}
