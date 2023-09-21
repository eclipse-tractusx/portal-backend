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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Type of a companyApplication
/// </summary>
public class CompanyApplicationType
{
    /// <summary>
    /// Internal constructor, only for EF
    /// </summary>
    private CompanyApplicationType()
    {
        Label = null!;
        CompanyApplications = new HashSet<CompanyApplication>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="CompanyApplicationType"/> and initializes the id and label 
    /// </summary>
    /// <param name="companyApplicationTypeId">The CompanyApplicationTypeId</param>
    public CompanyApplicationType(CompanyApplicationTypeId companyApplicationTypeId) : this()
    {
        Id = companyApplicationTypeId;
        Label = companyApplicationTypeId.ToString();
    }

    /// <summary>
    /// Id of the type
    /// </summary>
    public CompanyApplicationTypeId Id { get; private set; }

    /// <summary>
    /// The type as string 
    /// </summary>
    [MaxLength(255)]
    public string Label { get; private set; }

    public virtual ICollection<CompanyApplication> CompanyApplications { get; private set; }
}
