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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class CompanyCredentialDetail : IBaseEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public CredentialTypeId CredentialTypeId { get; set; }
    public UseCaseParticipationStatusId UseCaseParticipationStatusId { get; set; }
    public Guid DocumentId { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }

    // Navigation Properties
    public virtual Company? Company { get; set; }
    public virtual CredentialType? CredentialType { get; set; }
    public virtual UseCaseParticipationStatus? UseCaseParticipationStatus { get; set; }
    public virtual Document? Document { get; set; }
    public virtual CredentialAssignedUseCase? CredentialAssignedUseCase { get; private set; }
}
