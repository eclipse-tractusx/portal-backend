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
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditConsent20230412))]
public class Consent : IAuditableV1, IBaseEntity
{
    private Consent()
    {
        ConsentAssignedOffers = new HashSet<ConsentAssignedOffer>();
        ConsentAssignedOfferSubscriptions = new HashSet<ConsentAssignedOfferSubscription>();
    }

    public Consent(Guid id, Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, DateTimeOffset dateCreated)
        : this()
    {
        Id = id;
        AgreementId = agreementId;
        CompanyId = companyId;
        CompanyUserId = companyUserId;
        ConsentStatusId = consentStatusId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; set; }

    [MaxLength(255)]
    public string? Comment { get; set; }

    public ConsentStatusId ConsentStatusId { get; set; }

    [MaxLength(255)]
    public string? Target { get; set; }

    public Guid AgreementId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid CompanyUserId { get; set; }

    [AuditLastEditorV1]
    public Guid? LastEditorId { get; set; }

    // Navigation properties
    public virtual Agreement? Agreement { get; private set; }
    public virtual Company? Company { get; private set; }
    public virtual CompanyUser? CompanyUser { get; private set; }
    public virtual ConsentStatus? ConsentStatus { get; private set; }
    public virtual Document? Document { get; private set; }
    public virtual Identity? LastEditor { get; private set; }
    public virtual ICollection<ConsentAssignedOffer> ConsentAssignedOffers { get; private set; }
    public virtual ICollection<ConsentAssignedOfferSubscription> ConsentAssignedOfferSubscriptions { get; private set; }
}
