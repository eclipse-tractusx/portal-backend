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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class SharedIdpInstanceDetail(Guid id, string sharedIdpUrl, string clientId, byte[] clientSecret, byte[]? initializationVector, int encryptionMode, DateTimeOffset dateCreated) : IBaseEntity
{
    public Guid Id { get; private set; } = id;
    public string SharedIdpUrl { get; set; } = sharedIdpUrl;
    public string ClientId { get; set; } = clientId;

    public byte[] ClientSecret { get; set; } = clientSecret;
    public byte[]? InitializationVector { get; set; } = initializationVector;
    public int EncryptionMode { get; set; } = encryptionMode;
    public string? AuthRealm { get; set; }
    public bool UseAuthTrail { get; set; }
    public int RealmUsed { get; set; }
    public int MaxRealmCount { get; set; }
    public bool IsRunning { get; set; }
    public DateTimeOffset DateCreated { get; private set; } = dateCreated;

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }
    // Navigation properties
    public virtual ICollection<SharedIdpRealmMapping> SharedIdpRealmMappings { get; set; } = [];
}
