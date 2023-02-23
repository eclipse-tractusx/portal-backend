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

using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.Daps.Library;

/// <summary>
/// Service to handle communication with the connectors sd factory
/// </summary>
public interface IDapsService
{
    /// <summary>
    /// Registers the Connector at the connectorsSdFactory
    /// </summary>
    /// <param name="clientName">name of the client</param>
    /// <param name="connectorUrl">the connectors url with the bpn of the company append to it</param>
    /// <param name="businessPartnerNumber">the business partner number</param>
    /// <param name="formFile">The file</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <exception cref="ServiceException">throws an exception if the service call wasn't successfully</exception>
    Task<bool> EnableDapsAuthAsync(string clientName, string connectorUrl, string businessPartnerNumber, IFormFile formFile, CancellationToken cancellationToken);
}
