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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

/// <summary>
/// Input model defining all parameters for creating a connector in persistence layer.
/// </summary>
/// <param name="Name">Display name of the connector.</param>
/// <param name="ConnectorUrl"> URL of the connector.</param>
/// <param name="Location">Connector's location country code.</param>
/// <param name="Certificate">The certificate for the daps call.</param>
public record ConnectorInputModel(
    [MaxLength(255)] string Name,
    [MaxLength(255)] string ConnectorUrl,
    [StringLength(2, MinimumLength = 2)] string Location,
    IFormFile? Certificate);

/// <summary>
/// Input model defining all parameters for creating a connector in persistence layer.
/// </summary>
/// <param name="Name">Display name of the connector.</param>
/// <param name="ConnectorUrl"> URL of the connector..</param>
/// <param name="Location">Connector's location country code.</param>
/// <param name="ProviderBpn">Providing company's BPN.</param>
/// <param name="Certificate">The certificate for the daps call.</param>
public record ManagedConnectorInputModel(
    [MaxLength(255)] string Name,
    [MaxLength(255)] string ConnectorUrl,
    [StringLength(2, MinimumLength = 2)] string Location,
    string ProviderBpn,
    IFormFile? Certificate);

public record ConnectorRequestModel(
    [MaxLength(255)] string Name,
    [MaxLength(255)] string ConnectorUrl,
    ConnectorTypeId ConnectorType,
    [StringLength(2, MinimumLength = 2)] string Location,
    Guid Provider,
    Guid Host);
