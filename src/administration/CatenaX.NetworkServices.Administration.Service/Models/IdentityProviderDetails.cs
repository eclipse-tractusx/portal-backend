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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public record IdentityProviderDetails(Guid identityProviderId, string alias, IdentityProviderCategoryId identityProviderCategoryId, string displayName, string redirectUrl, bool enabled, IEnumerable<IdentityProviderMapperModel>  mappers)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdentityProviderDetailsOidc? oidc { get; init; } = null;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdentityProviderDetailsSaml? saml { get; init; } = null;
}

public record IdentityProviderDetailsOidc(string authorizationUrl, string clientId, IamIdentityProviderClientAuthMethod clientAuthMethod)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IamIdentityProviderSignatureAlgorithm? signatureAlgorithm { get; init; } = null;
}

public record IdentityProviderDetailsSaml(string serviceProviderEntityId, string singleSignOnServiceUrl);
