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

using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding.JsonHelper;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

public static class SeederHelper
{
    public static async Task<IList<T>> GetSeedData<T>(ILogger logger, string fileName, CancellationToken cancellationToken, params string[] additionalEnvironments) where T : class
    {
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        logger.LogInformation("Looking for files at {Location}", location);
        if (location == null)
        {
            throw new ConflictException($"No location found for assembly {Assembly.GetExecutingAssembly()}");
        }

        var data = new List<T>();
        data.AddRange(await GetDataFromFile<T>(logger, fileName, location, cancellationToken).ConfigureAwait(false));
        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 3
        };
        await Parallel.ForEachAsync(additionalEnvironments, parallelOptions, async (env, ct) =>
        {
            data.AddRange(await GetDataFromFile<T>(logger, fileName, location, ct, env));
        }).ConfigureAwait(false);

        return data;
    }

    private static async Task<List<T>> GetDataFromFile<T>(ILogger logger, string fileName, string location, CancellationToken cancellationToken, string? env = null) where T : class
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy()
        };
        options.Converters.Add(new JsonDateTimeOffsetConverter());

        var envPath = env == null ? null : $".{env}";
        // var fileName = typeof(T).Name.ToLower(); for now - out because of snake_case and custom portal db names
        var path = Path.Combine(location, @"Seeder/Data", $"{fileName}{envPath}.json");
        logger.LogInformation("Looking for file {Path}", path);
        if (!File.Exists(path)) return new List<T>();

        var data = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<T>>(data, options);
        return list ?? new List<T>();
    }
}