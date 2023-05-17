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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class UserSettings
{
	public UserSettings()
	{
		Portal = null!;
		PasswordReset = null!;
	}

	[Required]
	public UserSetting Portal { get; set; }
	public PasswordReset PasswordReset { get; set; }
	public int ApplicationsMaxPageSize { get; set; }

	/// <summary>
	/// Company User Status Id
	/// </summary>
	/// <value></value>
	[Required]
	public IEnumerable<CompanyUserStatusId> CompanyUserStatusIds { get; set; } = null!;

	/// <summary>
	/// Company User Status Id
	/// </summary>
	[Required]
	public IDictionary<string, IEnumerable<string>> UserAdminRoles { get; set; } = null!;
}

public class UserSetting
{
	public UserSetting()
	{
		KeycloakClientID = null!;
		BasePortalAddress = null!;
	}

	[Required(AllowEmptyStrings = false)]
	public string KeycloakClientID { get; set; }

	[Required(AllowEmptyStrings = false)]
	public string BasePortalAddress { get; set; }
}

public class PasswordReset
{
	public int NoOfHours { get; set; }
	public int MaxNoOfReset { get; set; }
}

public static class UserSettingsExtension
{
	public static IServiceCollection ConfigureUserSettings(
		this IServiceCollection services,
		IConfigurationSection section)
	{
		services.AddOptions<UserSettings>()
			.Bind(section)
			.ValidateDataAnnotations()
			.ValidateOnStart();
		return services;
	}
}
