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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing agreements on the persistence layer.
/// </summary>
public interface IAgreementRepository
{
    /// <summary>
    /// Checks whether the agreement with the given id exists. 
    /// </summary>
    /// <param name="agreementId">Id of the agreement</param>
    /// <returns>Returns <c></c></returns>
    Task<bool> CheckAgreementExistsAsync(Guid agreementId);
    
    /// <summary>
    /// Gets the agreement data that have an app id set
    /// </summary>
    /// <param name="iamUserId">Id of the user</param>
    /// <returns>Returns an async enumerable from agreement data</returns>
    IAsyncEnumerable<AgreementData> GetAgreementDataWithAppIdSet(string iamUserId);
}