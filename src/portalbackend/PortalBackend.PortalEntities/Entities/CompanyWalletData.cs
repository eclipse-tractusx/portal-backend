/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class CompanyWalletData : IBaseEntity
{
    public CompanyWalletData(Guid id, Guid companyId, string did, JsonDocument didDocument, string clientId, byte[] clientSecret, byte[]? initializationVector, int encryptionMode, string authenticationServiceUrl)
    {
        Id = id;
        CompanyId = companyId;
        Did = did;
        DidDocument = didDocument;
        ClientId = clientId;
        ClientSecret = clientSecret;
        InitializationVector = initializationVector;
        EncryptionMode = encryptionMode;
        AuthenticationServiceUrl = authenticationServiceUrl;
    }

    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public string Did { get; private set; }

    public string ClientId { get; private set; }

    public byte[] ClientSecret { get; set; }
    public byte[]? InitializationVector { get; set; }
    public int EncryptionMode { get; set; }

    public string AuthenticationServiceUrl { get; private set; }

    public virtual JsonDocument DidDocument { get; private set; }

    // Navigation properties
    public virtual Company? Company { get; private set; }
}
