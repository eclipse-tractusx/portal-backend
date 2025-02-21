/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library;

/// <summary>
/// Settings used in business logic concerning connectors.
/// </summary>
public class SdFactorySettings : KeyVaultAuthSettings
{
    /// <summary>
    ///  If <c>true</c> all sd factory calls are disabled and won't be called. The respective process steps will be skipped.
    /// </summary>
    public bool ClearinghouseConnectDisabled { get; set; }

    /// <summary>
    /// SD Factory endpoint for registering connectors.
    /// </summary>
    [Required]
    public string SdFactoryUrl { get; set; } = null!;
}
