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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

/// <summary>
/// Definition for a custom seeders
/// </summary>
/// <remarks>
/// The implementation of the <see cref="ICustomSeeder"/> is automatically done by adding <seealso cref="DatabaseInitializerExtensions"/> 
/// </remarks>
public interface ICustomSeeder
{
    /// <summary>
    /// Defines the order of the seeders execution.
    /// Should be a positive integer, where 1 is the first Seeder that runs.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Executes the logic for the seeder. This method is called by <seealso cref="CustomSeederRunner.RunSeedersAsync"/>
    /// </summary>
    /// <remarks>This method also executes a SaveChanges on the database</remarks>
    /// <param name="cancellationToken">CancellationToken</param>
    Task ExecuteAsync(CancellationToken cancellationToken);
}
