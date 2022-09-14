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

using System.ComponentModel.DataAnnotations;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Auditing;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.AuditEntities;

/// <summary>
/// Audit entity for <see cref="CompanyUser"/> only needed for configuration purposes
/// </summary>
public class AuditCompanyUserCplp1254 : IAuditEntity
{
    /// <inheritdoc />
    public Guid AuditId { get; set; }

    public Guid Id { get; set; }

    public DateTimeOffset DateCreated { get; private set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? Firstname { get; set; }

    public byte[]? Lastlogin { get; set; }

    [MaxLength(255)]
    public string? Lastname { get; set; }

    public Guid CompanyId { get; set; }

    public CompanyUserStatusId CompanyUserStatusId { get; set; }

    /// <inheritdoc />
    public Guid? LastEditorId { get; set; }

    /// <inheritdoc />
    public AuditOperationId AuditOperationId { get; set; }
    
    /// <inheritdoc />
    public new DateTimeOffset DateLastChanged { get; set; }
}