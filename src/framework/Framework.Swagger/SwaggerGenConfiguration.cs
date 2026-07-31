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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Swagger;

public static class SwaggerGenConfiguration
{
    public static void SetupSwaggerGen<TProgram>(SwaggerGenOptions c, string version)
    {
        var assemblyName = typeof(TProgram).Assembly.FullName?.Split(',')[0];

        c.SwaggerDoc(version, new OpenApiInfo { Title = assemblyName, Version = version });

        c.MapType<IFormFile>(() => new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" });
        c.SupportNonNullableReferenceTypes();

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token in the text input below.\r\n\r\nExample: \"eyJhbGc...\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });
        c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
            }
        });

        try
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, assemblyName + ".xml");
            c.IncludeXmlComments(filePath);
        }
        catch (Exception e)
        {
            throw new ConfigurationException("error configuring swagger xmldocumentation", e);
        }

        c.OperationFilter<BaseStatusCodeFilter>();
        c.OperationFilter<AddAuthFilter>();
    }
}
