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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class OnboardingServiceProviderDetail
{
    public OnboardingServiceProviderDetail()
    {
        CallbackUrl = null!;
        AuthUrl = null!;
        ClientId = null!;
        ClientSecret = null!;
    }

    public OnboardingServiceProviderDetail(Guid companyId, string callbackUrl, string authUrl, string clientId, byte[] clientSecret)
        : this()
    {
        CompanyId = companyId;
        CallbackUrl = callbackUrl;
        AuthUrl = authUrl;
        ClientId = clientId;
        ClientSecret = clientSecret;
    }

    public Guid CompanyId { get; set; }

    public string CallbackUrl { get; set; }

    public string AuthUrl { get; set; }

    public string ClientId { get; set; }

    public byte[] ClientSecret { get; set; }

    public virtual Company? Company { get; private set; }
}
