/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

public class IdentityProviderProvisioningService(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager) : IIdentityProviderProvisioningService
{
    public Task<string?> GetIdentityProviderDisplayName(string idpAlias) =>
        provisioningManager.GetCentralIdentityProviderDisplayName(idpAlias);

    public async Task UpdateCompanyNameInSharedIdentityProvider(Guid companyId, string companyName)
    {
        var idpAlias = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetSharedIdentityProviderIamAliasDataUntrackedAsync(companyId).ConfigureAwait(false) ?? throw new ConflictException($"company {companyId} is not associated with any shared idp");
        await provisioningManager.UpdateSharedIdentityProviderAsync(idpAlias, companyName).ConfigureAwait(false);
        await provisioningManager.UpdateOrCreateCentralIdentityProviderOrganisationMapperAsync(idpAlias, companyName).ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
