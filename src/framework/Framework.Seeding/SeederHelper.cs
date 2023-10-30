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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding.JsonHelper;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

public static class SeederHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        Converters = { new JsonDateTimeOffsetConverter() }
    };

    public static async Task<IList<T>> GetSeedData<T>(ILogger logger, string fileName, IEnumerable<string> dataPaths, CancellationToken cancellationToken, params string[] additionalEnvironments) where T : class
    {
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        logger.LogInformation("Looking for files at {Location}", location);
        if (location == null)
        {
            throw new ConflictException($"No location found for assembly {Assembly.GetExecutingAssembly()}");
        }

        var data = new ConcurrentBag<T>();
        var results = await Task.WhenAll(dataPaths.Select(path => GetDataFromFile<T>(logger, fileName, location, path, cancellationToken))).ConfigureAwait(false);
        foreach (var entry in results.SelectMany(item => item))
        {
            data.Add(entry);
        }

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 3
        };
        await Parallel.ForEachAsync(additionalEnvironments, parallelOptions, async (env, ct) =>
        {
            var results = await Task.WhenAll(dataPaths.Select(path => GetDataFromFile<T>(logger, fileName, location, path, ct, env))).ConfigureAwait(false);
            foreach (var entry in results.SelectMany(item => item))
            {
                data.Add(entry);
            }
        }).ConfigureAwait(false);

        return data.ToList();
    }

    private static async Task<List<T>> GetDataFromFile<T>(ILogger logger, string fileName, string location, string dataPath, CancellationToken cancellationToken, string? env = null) where T : class
    {
        var envPath = env == null ? null : $".{env}";
        var path = Path.Combine(location, dataPath, $"{fileName}{envPath}.json");
        logger.LogInformation("Looking for file {Path}", path);
        if (!File.Exists(path))
            return new List<T>();

        var data = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<T>>(data, Options);
        return list ?? new List<T>();
    }
}
