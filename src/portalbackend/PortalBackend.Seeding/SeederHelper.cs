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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

public static class SeederHelper
{
    public static async Task<IList<T>?> GetSeedData<T>(CancellationToken cancellationToken) where T : class
    {
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (location == null)
        {
            throw new ConflictException($"No location found for assembly {Assembly.GetExecutingAssembly()}");
        }

        var path = Path.Combine(location, @"Seeder\Data", $"{typeof(T).Name.ToLower()}.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonDateTimeOffsetConverter());

        var data = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<List<T>>(data, options);
    }
}