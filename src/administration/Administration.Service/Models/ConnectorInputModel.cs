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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

/// <summary>
/// Input model defining all parameters for creating a connector in persistence layer.
/// </summary>
/// <param name="Name">Display name of the connector.</param>
/// <param name="ConnectorUrl"> URL of the connector..</param>
/// <param name="Type">Connector type.</param>
/// <param name="Status">Connector status.</param>
/// <param name="Location">Connector's location country code.</param>
/// <param name="Provider">Providing company's ID..</param>
/// <param name="Host">Hosting company's ID.</param>
public record ConnectorInputModel(
    [MaxLength(255)] string Name,
    [MaxLength(255)] string ConnectorUrl,
    ConnectorTypeId Type,
    ConnectorStatusId Status,
    [StringLength(2, MinimumLength = 2)] string Location,
    Guid Provider,
    Guid? Host);

/// <summary>
/// Input model defining all parameters for creating a connector in persistence layer.
/// </summary>
public record ManagedConnectorInputModel : ConnectorInputModel
{
    /// <summary>
    /// Input model defining all parameters for creating a connector in persistence layer.
    /// </summary>
    /// <param name="name">Display name of the connector.</param>
    /// <param name="connectorUrl"> URL of the connector..</param>
    /// <param name="type">Connector type.</param>
    /// <param name="status">Connector status.</param>
    /// <param name="location">Connector's location country code.</param>
    /// <param name="provider">Providing company's ID..</param>
    /// <param name="host">Hosting company's ID.</param>
    public ManagedConnectorInputModel(string name,
        string connectorUrl,
        ConnectorTypeId type,
        ConnectorStatusId status,
        string location,
        Guid provider,
        Guid host) 
        : base(name, connectorUrl, type, status, location, provider, host)
    {
        this.Host = host;
    }

    public new Guid Host { get; }
}
