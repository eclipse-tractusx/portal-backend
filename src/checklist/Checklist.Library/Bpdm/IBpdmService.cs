/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Bpdm.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Bpdm;

/// <summary>
/// Service to call the BPDM endpoints
/// </summary>
public interface IBpdmService
{
    /// <summary>
    /// Triggers the bpn data push
    /// </summary>
    /// <param name="data">The bpdm data</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Returns <c>true</c> if the service call was successful, otherwise <c>false</c></returns>
    Task<bool> TriggerBpnDataPush(BpdmTransferData data, CancellationToken cancellationToken);
}
