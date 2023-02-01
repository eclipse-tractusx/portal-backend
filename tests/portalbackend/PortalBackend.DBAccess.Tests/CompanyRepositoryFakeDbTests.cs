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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="CompanyRepository"/>
/// </summary>
public class CompanyRepositoryFakeDbTests
{
    private readonly IFixture _fixture;
    private readonly PortalDbContext _contextFake;
    private readonly Guid _validCompanyId = Guid.NewGuid(); 
    private readonly Guid _companyWithoutBpnId = Guid.NewGuid(); 
    private readonly List<Company> _companies = new();

    public CompanyRepositoryFakeDbTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _contextFake = A.Fake<PortalDbContext>();

        SetupCompanyDb();
    }

    #region GetConnectorCreationCompanyDataAsync

    [Fact]
    public async Task GetConnectorCreationCompanyDataAsync_WithValidCompanyAndBpnRequested_ReturnsBpn()
    {
        // Arrange
        var sut = new CompanyRepository(_contextFake);

        // Act
        var result = await sut.GetCompanyBpnByIdAsync(_validCompanyId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetConnectorCreationCompanyDataAsync_WithNotExistingCompany_ReturnsNull()
    {
        // Arrange
        var sut = new CompanyRepository(_contextFake);

        // Act
        var result = await sut.GetCompanyBpnByIdAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectorCreationCompanyDataAsync_WithCompanyWithoutBpnAndBpnRequested_ReturnsNullBpn()
    {
        // Arrange
        var sut = new CompanyRepository(_contextFake);

        // Act
        var result = await sut.GetCompanyBpnByIdAsync(_companyWithoutBpnId).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    private void SetupCompanyDb()
    {
        _companies.Add(new Company(_validCompanyId, "testName", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
        {
            BusinessPartnerNumber = "BUISNESSPARTNERNO"
        });
        _companies.Add(new Company(_companyWithoutBpnId, "withoutBpn", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow));
        
        A.CallTo(() => _contextFake.Companies).Returns(_companies.AsFakeDbSet());
    }

    #region GetAllMemberCompaniesBPN

    [Fact]
    public async Task GetAllMemberCompaniesBPN__ReturnsBPNList()
    {
        // Arrange
        _fixture.Inject(_contextFake);
        var sut = _fixture.Create<CompanyRepository>();

        // Act
        var results = await sut.GetAllMemberCompaniesBPNAsync().ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
    }
     #endregion
}
