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

/// <summary>
/// Detail data for the app subscription
/// </summary>

[AuditEntityV1(typeof(AuditAppSubscriptionDetail20221118))]
public class AppSubscriptionDetail : IAuditableV1, IBaseEntity
{
    /// <summary>
    /// Only needed for ef
    /// </summary>
    public AppSubscriptionDetail()
    { }

    /// <summary>
    /// Creates a new instance of <see cref="AppSubscriptionDetail"/>
    /// </summary>
    /// <param name="id">Id of the entity</param>
    /// <param name="offerSubscriptionId">Id of the offer subscription</param>
    public AppSubscriptionDetail(Guid id, Guid offerSubscriptionId)
        : this()
    {
        this.Id = id;
        this.OfferSubscriptionId = offerSubscriptionId;
    }

    /// <summary>
    /// Id of the entity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Id of the Offer Subscription
    /// </summary>
    public Guid OfferSubscriptionId { get; set; }

    /// <summary>
    /// ID of the app instance.
    /// </summary>
    public Guid? AppInstanceId { get; set; }

    [MaxLength(255)]
    public string? AppSubscriptionUrl { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // navigational properties

    /// <summary>
    /// Subscribing app instance.
    /// </summary>
    public virtual AppInstance? AppInstance { get; private set; }

    /// <summary>
    /// Subscription of an offer
    /// </summary>
    public virtual OfferSubscription? OfferSubscription { get; private set; }
    public virtual Identity? LastEditor { get; private set; }
}
