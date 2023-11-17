/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;

/// <summary>
/// Audit entity for <see cref="OfferSubscription"/> relationship between companies and apps.
/// </summary>
public class AuditOfferSubscription20231115 : IAuditEntityV1
{
    /// <inheritdoc />
    [Key]
    public Guid AuditV1Id { get; set; }

    public Guid Id { get; set; }

    /// <summary>
    /// ID of the company subscribing an app.
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// ID of the apps subscribed by a company.
    /// </summary>
    public Guid? OfferId { get; set; }

    /// <summary>
    /// ID of the app subscription status.
    /// </summary>
    public OfferSubscriptionStatusId? OfferSubscriptionStatusId { get; set; }

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
    public Guid? RequesterId { get; set; }

    public Guid? LastEditorId { get; set; }

    public Guid? ProcessId { get; set; }

    public DateTimeOffset? DateCreated { get; private set; }

    /// <inheritdoc />
    public Guid? AuditV1LastEditorId { get; set; }

    /// <inheritdoc />
    public DateTimeOffset AuditV1DateLastChanged { get; set; }

    /// <inheritdoc />
    public AuditOperationId AuditV1OperationId { get; set; }
}
