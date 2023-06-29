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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

/// <summary>
/// Executes the custom seeders
/// </summary>
internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;
    private readonly ILogger<CustomSeederRunner> _logger;
    private readonly SeederSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="CustomSeederRunner"/>
    /// Retrieves all implementations of <seealso cref="ICustomSeeder"/>
    /// </summary>
    /// <param name="serviceProvider">The service provider with the CustomSeeder registrations</param>
    /// <param name="logger">Logger</param>
    /// <param name="options">The options</param>
    public CustomSeederRunner(IServiceProvider serviceProvider, ILogger<CustomSeederRunner> logger, IOptions<SeederSettings> options) =>
        (_seeders, _logger, _settings) = (serviceProvider.GetServices<ICustomSeeder>().ToArray(), logger, options.Value);

    /// <summary>
    /// Executes all registered seeders in the order they have defined
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Found {SeederCount} custom seeder", _seeders.Length);
        if (!_settings.DataPaths.Any())
        {
            _logger.LogInformation("There a no data paths configured, therefore no seeding will be executed");
            return;
        }

        foreach (var seeder in _seeders.OrderBy(x => x.Order))
        {
            await seeder.ExecuteAsync(cancellationToken);
        }
    }
}
