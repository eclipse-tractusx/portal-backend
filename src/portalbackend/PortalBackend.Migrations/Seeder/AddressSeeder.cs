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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

namespace  Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;

public class AddressSeeder : ICustomSeeder
{
    private readonly PortalDbContext _context;
    private readonly ILogger<AddressSeeder> _logger;

    public AddressSeeder(PortalDbContext context, ILogger<AddressSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var addresses = await SeederHelper.GetSeedData<Address>(cancellationToken).ConfigureAwait(false);
        if (addresses != null)
        {
            _logger.LogInformation("Started to Seed Addresses");
            addresses = (from a in addresses
                join dba in _context.Addresses on a.Id equals dba.Id into t
                from ad in t.DefaultIfEmpty()
                where ad == null
                select a).ToList();
            _logger.LogInformation("Seeding {AddressCount} addresses", addresses.Count());
            await _context.Addresses.AddRangeAsync(addresses, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded Addresses");
        }
    }
}