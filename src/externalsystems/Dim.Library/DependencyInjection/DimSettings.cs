/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Dim.Library.DependencyInjection;

public class DimSettings : KeyVaultAuthSettings
{
    [Required(AllowEmptyStrings = false)]
    public string BaseAddress { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string UniversalResolverAddress { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string DidDocumentBaseLocation { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public int MaxValidationTimeInDays { get; set; }

    [Required]
    public int EncryptionConfigIndex { get; set; }

    [Required]
    [DistinctValues("x => x.Index")]
    public IEnumerable<EncryptionModeConfig> EncryptionConfigs { get; set; } = null!;
}
