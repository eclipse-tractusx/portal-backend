/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic
{
    public interface IPartnerNetworkBusinessLogic
    {
        /// <summary>
        /// Get all member activecompanies bpn
        /// </summary>
        /// <param name="bpnIds">Ids of BPN</param>
        IAsyncEnumerable<string> GetAllMemberCompaniesBPNAsync(IEnumerable<string>? bpnIds);

        /// <summary>
        /// Gets partner network data from BPN Pool
        /// </summary>
        /// <param name="page" example="0">The page of partner network data, default is 0.</param>
        /// <param name="size" example="10">Amount of partner network data, default is 10.</param>
        /// <param name="partnerNetworkRequest">The bpnls to get the selected record</param>
        /// <param name="token">Access token to access the partner network pool</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns a List of partner networks</returns>
        Task<PartnerNetworkResponse> GetPartnerNetworkDataAsync(int page, int size, PartnerNetworkRequest partnerNetworkRequest, string token, CancellationToken cancellationToken);
    }
}
