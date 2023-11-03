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

using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;

/// <summary>
/// Service for wallet related topics
/// </summary>
public interface ICustodianService
{
    /// <summary>
    /// Gets a wallet by the bpn
    /// </summary>
    /// <param name="bpn">bpn to get the wallet for</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Returns either the wallet for the bpn or null</returns>
    /// <exception cref="ServiceException"></exception>
    Task<WalletData> GetWalletByBpnAsync(string bpn, CancellationToken cancellationToken);

    /// <summary>
    /// Creates the wallet
    /// </summary>
    /// <param name="bpn">BusinessPartnerNumber of the company</param>
    /// <param name="name">the name of the company</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The content of the document</returns>
    Task<string> CreateWalletAsync(string bpn, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the membership for the given bpn
    /// </summary>
    /// <param name="bpn">bpn of the company</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<string> SetMembership(string bpn, CancellationToken cancellationToken);

    /// <summary>
    /// Issue a verifiable credential for a use case
    /// </summary>
    /// <param name="bpn">BusinessPartnerNumber of the company to issue the credentials for</param>
    /// <param name="useCaseDetailData">Additional data needed for the endpoint</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task TriggerFrameworkAsync(string bpn, UseCaseDetailData useCaseDetailData, CancellationToken cancellationToken);

    /// <summary>
    /// Issue a verifiable credential for a dismantler
    /// </summary>
    /// <param name="bpn">BusinessPartnerNumber of the company to issue the credentials for</param>
    /// <param name="credentialTypeId">The credential type id</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task TriggerDismantlerAsync(string bpn, VerifiedCredentialTypeId credentialTypeId, CancellationToken cancellationToken);
}
