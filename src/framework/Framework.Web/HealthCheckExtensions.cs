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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web;

public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static Task WriteResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        return context.Response.WriteAsync(JsonSerializer.Serialize(
            new
            {
                Status = report.Status.ToString(),
                Duration = report.TotalDuration,
                Info = report.Entries
                    .Select(e => new
                    {
                        Key = e.Key,
                        Description = e.Value.Description,
                        Duration = e.Value.Duration,
                        Status = Enum.GetName<HealthStatus>(e.Value.Status),
                        Error = e.Value.Exception?.Message,
                        Data = e.Value.Data
                    })
            },
            _serializerOptions));
    }

    public static void MapDefaultHealthChecks(this WebApplication app, IEnumerable<HealthCheckSettings> settings)
    {
        if (settings != null)
        {
            if (settings.Select(x => x.Path).Distinct().Count() < settings.Count())
            {
                throw new ConfigurationException($"HealthChecks mapping {string.Join(", ", settings.Select(x => x.Path))} contains ambiguous pathes");
            }
            foreach (var configured in settings)
            {
                app.MapHealthChecks(configured.Path, new()
                {
                    Predicate = registration => configured.Tags != null && configured.Tags.Intersect(registration.Tags).Any(),
                    ResponseWriter = WriteResponse
                });
            }
        }
    }
}
