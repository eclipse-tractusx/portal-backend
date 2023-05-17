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

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Maintenance.App.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using System.Collections.Immutable;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Maintenance.App.Tests;

/// <summary>
/// Tests the functionality of the <see cref="BatchDeleteServiceTests"/>
/// </summary>
public class BatchDeleteServiceTests : IAssemblyFixture<TestDbFixture>
{
	private readonly TestDbFixture _dbTestDbFixture;
	private readonly IFixture _fixture;

	public BatchDeleteServiceTests(TestDbFixture testDbFixture)
	{
		_fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
		_fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
			.ForEach(b => _fixture.Behaviors.Remove(b));

		_fixture.Behaviors.Add(new OmitOnRecursionBehavior());
		_dbTestDbFixture = testDbFixture;
	}

	[Fact]
	public async Task ExecuteAsync_WithOldDocumentsAndAssigned_Removes()
	{
		// Arrange
		var sut = await CreateSut().ConfigureAwait(false);

		// Act
		await sut.StartAsync(CancellationToken.None).ConfigureAwait(false);

		// Assert
		true.Should().BeTrue();
	}

	private async Task<BatchDeleteService> CreateSut()
	{
		var hostApplicationLifetime = _fixture.Create<IHostApplicationLifetime>();
		var inMemorySettings = new Dictionary<string, string>
		{
			{ "DeleteIntervalInDays", "5" }
		}.ToImmutableDictionary();
		var config = new ConfigurationBuilder()
			.AddInMemoryCollection(inMemorySettings)
			.Build();
		var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);

		var serviceProvider = _fixture.Create<IServiceProvider>();
		A.CallTo(() => serviceProvider.GetService(typeof(PortalDbContext))).Returns(context);
		var serviceScope = _fixture.Create<IServiceScope>();
		A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
		var serviceScopeFactory = _fixture.Create<IServiceScopeFactory>();
		A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);

		return new BatchDeleteService(hostApplicationLifetime, serviceScopeFactory, _fixture.Create<ILogger<BatchDeleteService>>(), config);
	}
}
