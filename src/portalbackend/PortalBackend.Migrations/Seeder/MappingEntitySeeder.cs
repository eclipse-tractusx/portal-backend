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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

namespace  Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;

/// <summary>
/// Seeder to seed the mapping entities (those with a combined primary key)
/// </summary>
public class MappingEntitySeeder : ICustomSeeder
{
    private readonly PortalDbContext _context;
    private readonly ILogger<MappingEntitySeeder> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    public MappingEntitySeeder(PortalDbContext context, ILogger<MappingEntitySeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var data = await SeederHelper.GetSeedData<AgreementAssignedOffer>("agreement_assigned_offers", cancellationToken, "consortia").ConfigureAwait(false);
        await SeedAgreementAssignedOffers(data, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SeedAgreementAssignedOffers(IList<AgreementAssignedOffer> data, CancellationToken cancellationToken)
    {
        if (data.Any())
        {
            var typeName = nameof(AgreementAssignedOffer);
            _logger.LogInformation("Started to Seed {TableName}", typeName);
            data = (from d in data
                join dbData in _context.Set<AgreementAssignedOffer>() 
                    on new { d.AgreementId, d.OfferId } equals new { dbData.AgreementId, dbData.OfferId } into t
                from x in t.DefaultIfEmpty()
                where x == null
                select d).ToList();
            _logger.LogInformation("Seeding {DataCount} {TableName}", data.Count, typeName);
            await _context.Set<AgreementAssignedOffer>().AddRangeAsync(data, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Seeded {TableName}", typeName);
        }
    }
}