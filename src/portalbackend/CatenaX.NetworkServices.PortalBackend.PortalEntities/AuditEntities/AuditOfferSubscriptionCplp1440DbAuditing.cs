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
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;

/// <summary>
/// Audit entity for App subscription relationship between companies and apps.
/// </summary>
public class AuditOfferSubscriptionCplp1440DbAuditing : IAuditEntity
{
    /// <summary>
    /// Only used for the audit table
    /// </summary>
    public AuditOfferSubscriptionCplp1440DbAuditing()
    {
    }

    /// <inheritdoc />
    public Guid AuditId { get; set; }

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
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Additional description for clarification
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Id of the app requester 
    /// </summary>
    public Guid RequesterId { get; set; }
    
    /// <inheritdoc />
    public Guid? LastEditorId { get; set; }

    /// <inheritdoc />
    public DateTimeOffset DateLastChanged { get; set; }

    /// <inheritdoc />
    public AuditOperationId AuditOperationId { get; set; }
}
