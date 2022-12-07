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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Service to handle communication with the connectors sd factory
/// </summary>
public interface ISdFactoryService
{
    /// <summary>
    /// Registers the Connector at the connectorsSdFactory
    /// </summary>
    /// <param name="connectorRequestModel">the connector request model</param>
    /// <param name="accessToken">the access token</param>
    /// <param name="businessPartnerNumber">the bpn</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ServiceException">throws an exception if the service call wasn't successfully</exception>
    Task<Guid> RegisterConnectorAsync(ConnectorRequestModel connectorRequestModel, string accessToken, string businessPartnerNumber, CancellationToken cancellationToken);

    Task<Guid> RegisterSelfDescriptionAsync(string accessToken, Guid applicationId, string countryCode, string businessPartnerNumber, CancellationToken cancellationToken);
}
