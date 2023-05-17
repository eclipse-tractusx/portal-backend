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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

public class JwtBearerConfigurationHealthCheck : IHealthCheck
{
	private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

	public JwtBearerConfigurationHealthCheck(IOptions<JwtBearerOptions> jwtOptions)
	{
		var options = jwtOptions.Value;
		if (!options.RequireHttpsMetadata)
		{
			options.BackchannelHttpHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (_, _, _, _) => true
			};
		}
		_configurationManager = options.BackchannelHttpHandler == null
			? new ConfigurationManager<OpenIdConnectConfiguration>(
				options.MetadataAddress,
				new OpenIdConnectConfigurationRetriever())
			: new ConfigurationManager<OpenIdConnectConfiguration>(
				options.MetadataAddress,
				new OpenIdConnectConfigurationRetriever(),
				new HttpClient(options.BackchannelHttpHandler));
	}

	public async Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		try
		{
			await _configurationManager.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
			return HealthCheckResult.Healthy();
		}
		catch (Exception e)
		{
			return HealthCheckResult.Unhealthy(exception: e);
		}
	}
}
