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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class OnboardingServiceProviderDetail : IBaseEntity
{
    public OnboardingServiceProviderDetail(Guid id, Guid companyId, string callbackUrl, string authUrl, string clientId, byte[] clientSecret, byte[]? initializationVector, int encryptionMode)
    {
        Id = id;
        CompanyId = companyId;
        CallbackUrl = callbackUrl;
        AuthUrl = authUrl;
        ClientId = clientId;
        ClientSecret = clientSecret;
        InitializationVector = initializationVector;
        EncryptionMode = encryptionMode;
    }

    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public string CallbackUrl { get; set; }

    public string AuthUrl { get; set; }

    public string ClientId { get; set; }

    public byte[] ClientSecret { get; set; }
    public byte[]? InitializationVector { get; set; }
    public int EncryptionMode { get; set; }

    public virtual Company? Company { get; private set; }
}
