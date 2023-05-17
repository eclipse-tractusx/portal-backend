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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Swagger;

public class AddAuthFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var authorizeAttributes = context.MethodInfo
			.GetCustomAttributes(true)
			.Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? Array.Empty<object>())
			.OfType<AuthorizeAttribute>()
			.ToList();

		if (!authorizeAttributes.Any())
			return;

		var authorizationDescription = new StringBuilder(" (Authorization required");
		var policies = authorizeAttributes
			.Where(a => !string.IsNullOrEmpty(a.Roles))
			.Select(a => a.Roles)
			.OrderBy(role => role)
			.ToList();

		if (policies.Any())
		{
			authorizationDescription.Append($" - Roles: {string.Join(", ", policies)};");
		}

		operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse { Description = "The User is unauthorized" });
		operation.Summary += authorizationDescription.ToString().TrimEnd(';') + ")";
	}
}
