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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace  Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;

/// <summary>
/// Seeder to seed the language entities
/// </summary>
public class LanguageSeeder : ICustomSeeder
{
    private readonly PortalDbContext _context;
    private readonly ILogger<LanguageSeeder> _logger;
    private readonly SeederSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    /// <param name="options">The options</param>
    public LanguageSeeder(PortalDbContext context, ILogger<LanguageSeeder> logger, IOptions<SeederSettings> options)
    {
        _context = context;
        _logger = logger;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public int Order => 1;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var data = await SeederHelper.GetSeedData<Language>(_logger, "languages", cancellationToken, _settings.TestDataEnvironments.ToArray()).ConfigureAwait(false);
        if (data.Any())
        {
            const string typeName = nameof(Language);
            _logger.LogInformation("Started to Seed {TableName}", typeName);
            data = (from d in data
                join dbData in _context.Set<Language>() on d.ShortName equals dbData.ShortName into t
                from x in t.DefaultIfEmpty()
                where x == null
                select d).ToList();
            _logger.LogInformation("Seeding {DataCount} {TableName}", data.Count, typeName);
            await _context.Set<Language>().AddRangeAsync(data, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Seeded {TableName}", typeName);
        }
        
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}