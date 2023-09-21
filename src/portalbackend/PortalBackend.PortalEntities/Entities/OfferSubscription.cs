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

/// <summary>
/// App subscription relationship between companies and apps.
/// </summary>
[AuditEntityV1(typeof(AuditOfferSubscription20230317))]
public class OfferSubscription : IAuditableV1, IBaseEntity
{
    /// <summary>
    /// Only used for the audit table
    /// </summary>
    public OfferSubscription()
    {
        ConsentAssignedOfferSubscriptions = new HashSet<ConsentAssignedOfferSubscription>();
        CompanyServiceAccounts = new HashSet<CompanyServiceAccount>();
        ConnectorAssignedOfferSubscriptions = new HashSet<ConnectorAssignedOfferSubscription>();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id">Id of the entity..</param>
    /// <param name="offerId">Offer id.</param>
    /// <param name="companyId">Company id.</param>
    /// <param name="offerSubscriptionStatusId">app subscription status.</param>
    /// <param name="requesterId">Id of the requester</param>
    /// <param name="lastEditorId">Id of the editor</param>
    public OfferSubscription(Guid id, Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId)
        : this()
    {
        Id = id;
        OfferId = offerId;
        CompanyId = companyId;
        OfferSubscriptionStatusId = offerSubscriptionStatusId;
        RequesterId = requesterId;
    }

    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the company subscribing an app.
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// ID of the apps subscribed by a company.
    /// </summary>
    public Guid OfferId { get; set; }

    /// <summary>
    /// ID of the app subscription status.
    /// </summary>
    public OfferSubscriptionStatusId OfferSubscriptionStatusId { get; set; }

    /// <summary>
    /// Display Name for the company app combination
    /// </summary>
    [MaxLength(255)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Additional description for clarification
    /// </summary>
    [MaxLength(4096)]
    public string? Description { get; set; }

    /// <summary>
    /// Id of the app requester 
    /// </summary>
    public Guid RequesterId { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    public Guid? ProcessId { get; set; }

    // Navigation properties
    /// <summary>
    /// Subscribed app.
    /// </summary>
    public virtual Offer? Offer { get; private set; }

    /// <summary>
    /// Subscribing company.
    /// </summary>
    public virtual Company? Company { get; private set; }

    /// <summary>
    /// Assigned Process.
    /// </summary>
    public virtual Process? Process { get; private set; }

    /// <summary>
    /// Requester
    /// </summary>
    public virtual CompanyUser? Requester { get; private set; }
    public virtual Identity? LastEditor { get; private set; }

    /// <summary>
    /// Subscription status.
    /// </summary>
    public virtual OfferSubscriptionStatus? OfferSubscriptionStatus { get; private set; }

    public virtual AppSubscriptionDetail? AppSubscriptionDetail { get; private set; }

    public virtual OfferSubscriptionProcessData? OfferSubscriptionProcessData { get; private set; }

    public virtual ICollection<ConnectorAssignedOfferSubscription> ConnectorAssignedOfferSubscriptions { get; private set; }
    public virtual ICollection<ConsentAssignedOfferSubscription> ConsentAssignedOfferSubscriptions { get; private set; }
    public virtual ICollection<CompanyServiceAccount> CompanyServiceAccounts { get; private set; }
}
