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

using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

public class KeyVaultAuthSettings
{
    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; } = null!;

    [Required(AllowEmptyStrings = false)] 
    public string Password { get; set; } = null!;

    [Required(AllowEmptyStrings = false)] 
    public string ClientId { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string GrantType { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string ClientSecret { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string Scope { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string KeycloakTokenAddress { get; set; } = null!;
}