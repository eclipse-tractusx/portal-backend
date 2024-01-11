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

namespace Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library;

public interface IIdpManagement
{
    ValueTask<string> GetNextCentralIdentityProviderNameAsync();
    Task CreateCentralIdentityProviderAsync(string alias, string displayName);
    Task<(string ClientId, string Secret, string ServiceAccountUserId)> CreateSharedIdpServiceAccountAsync(string realm);
    ValueTask UpdateCentralIdentityProviderUrlsAsync(string alias, string organisationName, string loginTheme, string clientId, string secret);
    Task CreateCentralIdentityProviderOrganisationMapperAsync(string alias, string organisationName);
    Task CreateSharedRealmIdpClientAsync(string realm, string loginTheme, string organisationName, string clientId, string secret);
    ValueTask EnableCentralIdentityProviderAsync(string alias);
    Task AddRealmRoleMappingsToUserAsync(string serviceAccountUserId);
    Task CreateSharedClientAsync(string realm, string clientId, string secret);
}
