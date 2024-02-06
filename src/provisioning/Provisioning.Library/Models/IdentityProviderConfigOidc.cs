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

using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

public record IdentityProviderConfigOidc(string DisplayName, string RedirectUrl, string TokenUrl, string? LogoutUrl, string ClientId, string? ClientSecret, bool Enabled, string AuthorizationUrl, IamIdentityProviderClientAuthMethod ClientAuthMethod, IamIdentityProviderSignatureAlgorithm? SignatureAlgorithm);
public record IdentityProviderEditableConfigOidc(string Alias, string DisplayName, string MetadataUrl, IamIdentityProviderClientAuthMethod ClientAuthMethod, string ClientId, string? Secret = null, IamIdentityProviderSignatureAlgorithm? SignatureAlgorithm = null);
