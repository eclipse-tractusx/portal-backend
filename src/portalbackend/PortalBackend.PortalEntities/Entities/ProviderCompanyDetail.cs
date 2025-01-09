/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.AuditEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

[AuditEntityV1(typeof(AuditProviderCompanyDetail20250415))]
public class ProviderCompanyDetail(Guid id, Guid companyId, string autoSetupUrl, string authUrl, string clientId, byte[] clientSecret, int encryptionMode) : IAuditableV1, IBaseEntity
{
    public Guid Id { get; private set; } = id;

    public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

    public string AutoSetupUrl { get; set; } = autoSetupUrl;

    public string? AutoSetupCallbackUrl { get; set; }

    public Guid CompanyId { get; set; } = companyId;
    public string AuthUrl { get; set; } = authUrl;
    public string ClientId { get; set; } = clientId;

    public byte[] ClientSecret { get; set; } = clientSecret;
    public byte[]? InitializationVector { get; set; }
    public int EncryptionMode { get; set; } = encryptionMode;

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // Navigation properties
    public virtual Company? Company { get; private set; }
    public virtual Identity? LastEditor { get; private set; }
}
