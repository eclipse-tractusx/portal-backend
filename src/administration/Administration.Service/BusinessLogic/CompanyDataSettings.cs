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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataSettings
{
    public CompanyDataSettings()
    {
        UseCaseParticipationMediaTypes = null!;
        SsiCertificateMediaTypes = null!;
        CompanyCertificateMediaTypes = null!;
    }

    /// <summary>
    /// The media types that are allowed for the uploaded document for use case participation
    /// </summary>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<MediaTypeId> UseCaseParticipationMediaTypes { get; set; }

    /// <summary>
    /// The media types that are allowed for the uploaded document for ssi certificate
    /// </summary>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<MediaTypeId> SsiCertificateMediaTypes { get; set; }

    /// <summary>
    /// The media types that are allowed for the uploaded document for company certificate
    /// </summary>
    [Required]
    [EnumEnumeration]
    [DistinctValues]
    public IEnumerable<MediaTypeId> CompanyCertificateMediaTypes { get; set; }

    /// <summary>
    /// The maximum page size
    /// </summary>
    public int MaxPageSize { get; set; }
}

public static class CompanyDataSettingsExtensions
{
    public static IServiceCollection ConfigureCompanyDataSettings(
        this IServiceCollection services,
        IConfigurationSection section
    )
    {
        services.AddOptions<CompanyDataSettings>()
            .Bind(section)
            .ValidateDistinctValues(section)
            .ValidateEnumEnumeration(section)
            .ValidateOnStart();
        return services;
    }
}
