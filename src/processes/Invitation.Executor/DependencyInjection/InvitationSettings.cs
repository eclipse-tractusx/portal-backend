/********************************************************************************
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.DependencyInjection;

public class InvitationSettings
{
    [Required(AllowEmptyStrings = false)]
    public string RegistrationAppAddress { get; set; } = null!;

    [Required]
    [DistinctValues("x => x.ClientId")]
    public IEnumerable<UserRoleConfig> InvitedUserInitialRoles { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string InitialLoginTheme { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string PasswordResendAddress { get; set; } = null!;

    [Required]
    public int EncryptionConfigIndex { get; set; }

    [Required]
    [DistinctValues("x => x.Index")]
    public IEnumerable<EncryptionModeConfig> EncryptionConfigs { get; set; } = null!;
}

public static class InvitationSettingsExtension
{
    public static IServiceCollection ConfigureInvitationSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<InvitationSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateDistinctValues(section)
            .ValidateOnStart();
        return services;
    }
}
