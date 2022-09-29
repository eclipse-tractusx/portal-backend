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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// View model for connectors.
/// </summary>
public class ConnectorData
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="location">Location.</param>
    public ConnectorData(string name, string location)
    {
        Name = name;
        Location = location;
    }

    /// <summary>
    /// ID of the connector.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the connector.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Connector type.
    /// </summary>
    public ConnectorTypeId Type { get; set; }

    /// <summary>
    /// Country code of the connector's location.
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Location { get; set; }

    /// <summary>
    /// Connector status.
    /// </summary>
    public ConnectorStatusId Status { get; set; }
}
