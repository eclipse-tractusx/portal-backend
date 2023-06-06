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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// model for Use Case.
/// </summary>
public class UseCaseData
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    /// <param name="shortName">ShortName</param>
    public UseCaseData(Guid id, string name, string shortName)
    {
        Id = id;
        Name = name;
        ShortName = shortName;
    }

    /// <summary>
    /// Id of Use Case
    /// </summary>
    /// <value></value>
    [JsonPropertyName("useCaseId")]
    public Guid Id { get; private set; }

    /// <summary>
    /// Name of Use Case
    /// </summary>
    /// <value></value>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// ShortName of Use Case
    /// </summary>
    /// <value></value>
    [JsonPropertyName("shortname")]
    public string ShortName { get; set; }
}
