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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

internal class DbSeeder
{
	private readonly CustomSeederRunner _seederRunner;
	private readonly ILogger<DbSeeder> _logger;

	public DbSeeder(CustomSeederRunner seederRunner, ILogger<DbSeeder> logger)
	{
		_seederRunner = seederRunner;
		_logger = logger;
	}

	public async Task SeedDatabaseAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Run custom seeder");
		await _seederRunner.RunSeedersAsync(cancellationToken);
		_logger.LogInformation("Custom seeding finished");
	}
}
