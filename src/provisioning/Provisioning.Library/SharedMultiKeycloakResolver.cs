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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public class SharedMultiKeycloakResolver(IPortalRepositories portalRepositories, IKeycloakFactory keycloakFactory, IOptions<MultiKeycloakSettings> options) : ISharedMultiKeycloakResolver
{
    private static readonly string SharedIDP = "shared";
    private readonly MultiKeycloakSettings _settings = options.Value;

    public async Task<KeycloakClient> GetKeycloakClient(string? realmName)
    {
        if (_settings.IsSharedIdpMultiInstancesEnabled)
        {
            ArgumentNullException.ThrowIfNull(realmName);
            var (sharedIdpUrl, clientId, clientSecret, initializationVector, encryptionMode, authRealm, useAuthTrail) = await portalRepositories.GetInstance<IIdentityProviderRepository>()
             .GetSharedIdpInstanceByRealm(realmName);
            var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == encryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {encryptionMode} is not configured");
            var secret = CryptoHelper.Decrypt(clientSecret!, initializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

            return KeycloakClient.CreateWithClientId(sharedIdpUrl, clientId, secret, useAuthTrail, authRealm);
        }
        return keycloakFactory.CreateKeycloakClient(SharedIDP);
    }

    public async Task<KeycloakClient> GetKeycloakClient(string realmName, string clientId, string clientSecret)
    {
        if (_settings.IsSharedIdpMultiInstancesEnabled)
        {
            var (sharedIdpUrl, _, _, _, _, authRealm, useAuthTrail) = await portalRepositories.GetInstance<IIdentityProviderRepository>()
                 .GetSharedIdpInstanceByRealm(realmName);

            return KeycloakClient.CreateWithClientId(sharedIdpUrl, clientId, clientSecret, useAuthTrail, authRealm);
        }
        return keycloakFactory.CreateKeycloakClient(SharedIDP, clientId, clientSecret);
    }

    public KeycloakClient GetKeycloakClient(SharedIdpInstanceDetail instance)
    {
        if (!_settings.IsSharedIdpMultiInstancesEnabled)
            throw new InvalidOperationException("Use GetKeycloakClient(SharedIdpInstanceDetail instance) method when multi instance is enabled");

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == instance.EncryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {instance.EncryptionMode} is not configured");
        var secret = CryptoHelper.Decrypt(instance.ClientSecret!, instance.InitializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        return KeycloakClient.CreateWithClientId(instance.SharedIdpUrl, instance.ClientId, secret, instance.UseAuthTrail, instance.AuthRealm);
    }
    public async Task<KeycloakClient> ResolveAndAssignKeycloak(string realm)
    {
        if (_settings.IsSharedIdpMultiInstancesEnabled)
        {
            var multiIdpInstanceDetail = await FindAndCreateSharedIdpRealmMapping(realm);
            return CreateKeycloakClient(multiIdpInstanceDetail);
        }
        return keycloakFactory.CreateKeycloakClient(SharedIDP);
    }

    private KeycloakClient CreateKeycloakClient(SharedIdpInstanceDetail sharedIdpInstanceDetail)
    {
        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == sharedIdpInstanceDetail.EncryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {sharedIdpInstanceDetail.EncryptionMode} is not configured");
        var secret = CryptoHelper.Decrypt(sharedIdpInstanceDetail.ClientSecret, sharedIdpInstanceDetail.InitializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);
        return KeycloakClient.CreateWithClientId(sharedIdpInstanceDetail.SharedIdpUrl, sharedIdpInstanceDetail.ClientId, secret, sharedIdpInstanceDetail.UseAuthTrail, sharedIdpInstanceDetail.AuthRealm);
    }

    private async Task<SharedIdpInstanceDetail> FindAndCreateSharedIdpRealmMapping(string realmName)
    {
        var identityProviderRepository = portalRepositories.GetInstance<IIdentityProviderRepository>();
        var sharedIdpInstances = await identityProviderRepository.GetAllSharedIdpInstanceDetails();
        //find the available shared idp instance
        var availableInstance = sharedIdpInstances
                                .Where(x => x.RealmUsed < x.MaxRealmCount)
                                .OrderBy(x => x.RealmUsed)
                                .FirstOrDefault();

        if (availableInstance == default)
        {
            throw new InvalidOperationException("No available shared IDP instance found under realm threshold.");
        }
        var sharedIdpInstance = identityProviderRepository.CreateSharedIdpRealmMapping(availableInstance.Id, realmName);
        availableInstance.RealmUsed++;
        return availableInstance;
    }
}

