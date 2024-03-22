/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="LanguageRepository"/>
/// </summary>
public class LanguageRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public LanguageRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Get Language

    [Fact]
    public async Task GetLanguageAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.IsValidLanguageCode("de");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetLanguageAsync_WithNotExistingLanguage_ReturnsNull()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.IsValidLanguageCode("notExisting");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetLanguageCodes

    [Fact]
    public async Task GetLanguageCodesUntrackedAsync_ReturnsExpectedResult()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var languageCodes = await sut.GetLanguageCodesUntrackedAsync(new[]
        {
            "de",
            "en",
            "notExisting"
        }).ToListAsync();

        // Assert
        languageCodes.Should().HaveCount(2);
        languageCodes.Should().Contain("de");
        languageCodes.Should().Contain("en");
    }

    #endregion

    #region Setup

    private async Task<LanguageRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new LanguageRepository(context);
        return sut;
    }

    #endregion
}
