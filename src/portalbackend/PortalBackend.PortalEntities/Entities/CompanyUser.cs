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
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditCompanyUser20230522))]
public class CompanyUser : IBaseEntity, IAuditableV1
{
    public CompanyUser(Guid id)
    {
        Id = id;

        Consents = new HashSet<Consent>();
        Documents = new HashSet<Document>();
        Invitations = new HashSet<Invitation>();
        Offers = new HashSet<Offer>();
        SalesManagerOfOffers = new HashSet<Offer>();
        CompanyUserAssignedBusinessPartners = new HashSet<CompanyUserAssignedBusinessPartner>();
        Notifications = new HashSet<Notification>();
        RequestedSubscriptions = new HashSet<OfferSubscription>();
        CompanySsiDetails = new HashSet<CompanySsiDetail>();
        CompanyUserAssignedIdentityProviders = new HashSet<CompanyUserAssignedIdentityProvider>();
    }

    /// <inheritdoc />
    public Guid Id { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? Firstname { get; set; }

    public byte[]? Lastlogin { get; set; }

    [MaxLength(255)]
    public string? Lastname { get; set; }

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    public virtual Identity? Identity { get; set; }
    public virtual Identity? LastEditor { get; private set; }
    public virtual ICollection<Consent> Consents { get; private set; }
    public virtual ICollection<Document> Documents { get; private set; }
    public virtual ICollection<Invitation> Invitations { get; private set; }
    public virtual ICollection<Offer> Offers { get; private set; }
    public virtual ICollection<Offer> SalesManagerOfOffers { get; private set; }
    public virtual ICollection<CompanyUserAssignedBusinessPartner> CompanyUserAssignedBusinessPartners { get; private set; }
    public virtual ICollection<CompanyUserAssignedIdentityProvider> CompanyUserAssignedIdentityProviders { get; private set; }
    public virtual ICollection<Notification> Notifications { get; private set; }
    public virtual ICollection<OfferSubscription> RequestedSubscriptions { get; private set; }
    public virtual ICollection<CompanySsiDetail> CompanySsiDetails { get; private set; }
}
