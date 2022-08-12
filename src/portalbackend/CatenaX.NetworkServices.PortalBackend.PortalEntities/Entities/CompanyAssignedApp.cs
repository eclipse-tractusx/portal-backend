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
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// App subscription relationship between companies and apps.
/// </summary>
public class CompanyAssignedApp : IAuditable
{
    /// <summary>
    /// Only used for the audit table
    /// </summary>
    protected CompanyAssignedApp()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id">Id of the entity..</param>
    /// <param name="appId">App id.</param>
    /// <param name="companyId">Company id.</param>
    /// <param name="appSubscriptionStatusId">app subscription status.</param>
    /// <param name="requesterId">Id of the requester</param>
    public CompanyAssignedApp(Guid id, Guid appId, Guid companyId, AppSubscriptionStatusId appSubscriptionStatusId, Guid requesterId, Guid lastEditorId)
    {
        Id = id;
        AppId = appId;
        CompanyId = companyId;
        AppSubscriptionStatusId = appSubscriptionStatusId;
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
    public Guid AppId { get; private set; }

    /// <summary>
    /// ID of the app subscription status.
    /// </summary>
    public AppSubscriptionStatusId AppSubscriptionStatusId { get; set; }

    /// <summary>
    /// Id of the app requester 
    /// </summary>
    public Guid RequesterId { get; set; }
    
    /// <inheritdoc />
    public Guid? LastEditorId { get; set; }

    // Navigation properties
    /// <summary>
    /// Subscribed app.
    /// </summary>
    public virtual App? App { get; private set; }
    /// <summary>
    /// Subscribing company.
    /// </summary>
    public virtual Company? Company { get; private set; }
    /// <summary>
    /// Subscription status.
    /// </summary>
    public virtual AppSubscriptionStatus? AppSubscriptionStatus { get; private set; }
}
