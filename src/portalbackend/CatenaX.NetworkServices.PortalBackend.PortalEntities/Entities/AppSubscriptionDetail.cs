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

using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Detail data for the app subscription
/// </summary>
public class AppSubscriptionDetail
{
    /// <summary>
    /// Only needed for ef
    /// </summary>
    private AppSubscriptionDetail()
    { }

    /// <summary>
    /// Creates a new instance of <see cref="AppSubscriptionDetail"/>
    /// </summary>
    /// <param name="id">Id of the entity</param>
    public AppSubscriptionDetail(Guid id) 
        : this()
    {
        this.Id = id;
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

    /// <summary>
    /// Subscribing app instance.
    /// </summary>
    public virtual AppInstance? AppInstance { get; private set; }

    /// <summary>
    /// Subscription of an offer
    /// </summary>
    public virtual OfferSubscription? OfferSubscription { get; private set; }
}