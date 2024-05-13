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

using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Dim.Library;

/// <summary>
/// Service for wallet related topics
/// </summary>
public interface IDimService
{
    /// <summary>
    /// Creates the wallet
    /// </summary>
    /// <param name="companyName">Company Name for the wallet</param>
    /// <param name="bpn">Bpn of the company for the wallet</param>
    /// <param name="didDocumentLocation">Location of the did document</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>The content of the document</returns>
    Task<bool> CreateWalletAsync(string companyName, string bpn, string didDocumentLocation, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the did using the universal resolver
    /// </summary>
    /// <param name="did">The did that should be checked</param>
    /// <param name="cancellationToken">The CancellationToken</param>
    /// <returns><c>true</c> if the did is valid, otherwise <c>false</c></returns>
    Task<bool> ValidateDid(string did, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a technical user for the wallet of the company with the given bpn
    /// </summary>
    /// <param name="bpn">Bpn of the company the technical user should be created for</param>
    /// <param name="technicalUserData">Data for the technical user creation</param>
    /// <param name="cancellationToken">The CancellationToken</param>
    Task CreateTechnicalUser(string bpn, TechnicalUserData technicalUserData, CancellationToken cancellationToken);
}
