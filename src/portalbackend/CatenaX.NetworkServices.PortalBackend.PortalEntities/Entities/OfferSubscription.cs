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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Auditing;
using System.ComponentModel.DataAnnotations;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// App subscription relationship between companies and apps.
/// </summary>
public class OfferSubscription : IAuditable
{
    /// <summary>
    /// Only used for the audit table
    /// </summary>
    protected OfferSubscription()
    {
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
    public OfferSubscription(Guid id, Guid offerId, Guid companyId, OfferSubscriptionStatusId offerSubscriptionStatusId, Guid requesterId, Guid lastEditorId)
    {
        Id = id;
        OfferId = offerId;
        CompanyId = companyId;
        OfferSubscriptionStatusId = offerSubscriptionStatusId;
        RequesterId = requesterId;
        LastEditorId = lastEditorId;
    }

    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the company subscribing an app.
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// ID of the apps subscribed by a company.
    /// </summary>
    public Guid OfferId { get; private set; }

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
    
    /// <inheritdoc />
    public Guid? LastEditorId { get; set; }

    public Guid? AppSubscriptionDetailId { get; set; }

    public virtual AppSubscriptionDetail? AppSubscriptionDetail { get; private set; }

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
    /// Subscription status.
    /// </summary>
    public virtual OfferSubscriptionStatus? OfferSubscriptionStatus { get; private set; }
}