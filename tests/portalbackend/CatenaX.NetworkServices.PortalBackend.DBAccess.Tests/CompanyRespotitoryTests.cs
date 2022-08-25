﻿/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared;
using CatenaX.NetworkServices.Tests.Shared.Extensions;
using FakeItEasy;
using FakeItEasy.Sdk;
using FluentAssertions;
using Xunit;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="CompanyRepository"/>
/// </summary>
public class CompanyRepositoryTests
{
    private readonly IFixture _fixture;
    private readonly PortalDbContext _contextFake;
    private readonly Guid _validCompanyId = Guid.NewGuid(); 
    private readonly Guid _companyWithoutBpnId = Guid.NewGuid(); 
    private readonly List<Company> _companies = new();

    public CompanyRepositoryTests()
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
    public async Task GetConnectorCreationCompanyDataAsync_WithValidCompanyAndBpnRequested_ReturnsCompanyWithBpn()
    {
        // Arrange
        _fixture.Inject(_contextFake);
        var parameter = new List<(Guid companyId, bool bpnRequested)>
        {
            new(_validCompanyId, true)
        };
        var sut = _fixture.Create<CompanyRepository>();

        // Act
        var results = await sut.GetConnectorCreationCompanyDataAsync(parameter).ToListAsync();

        // Assert
        results.Should().ContainSingle();
        results.All(x => x.BusinessPartnerNumber is not null).Should().BeTrue();
    }

    [Fact]
    public async Task GetConnectorCreationCompanyDataAsync_WithValidCompanyAndBpnRequestedFalse_ReturnsCompanyWithoutBpn()
    {
        // Arrange
        _fixture.Inject(_contextFake);
        var parameter = new List<(Guid companyId, bool bpnRequested)>
        {
            new(_validCompanyId, false)
        };
        var sut = _fixture.Create<CompanyRepository>();

        // Act
        var results = await sut.GetConnectorCreationCompanyDataAsync(parameter).ToListAsync();

        // Assert
        results.Should().ContainSingle();
        results.All(x => x.BusinessPartnerNumber is null).Should().BeTrue();
    }

    [Fact]
    public async Task GetConnectorCreationCompanyDataAsync_WithNotExistingCompany_ReturnsEmptyList()
    {
        // Arrange
        _fixture.Inject(_contextFake);
        var parameter = new List<(Guid companyId, bool bpnRequested)>
        {
            new(Guid.NewGuid(), true)
        };
        var sut = _fixture.Create<CompanyRepository>();

        // Act
        var results = await sut.GetConnectorCreationCompanyDataAsync(parameter).ToListAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConnectorCreationCompanyDataAsync_WithCompanyWithoutBpnAndBpnRequested_ReturnsNullBpn()
    {
        // Arrange
        _fixture.Inject(_contextFake);
        var parameter = new List<(Guid companyId, bool bpnRequested)>
        {
            new(_companyWithoutBpnId, true)
        };
        var sut = _fixture.Create<CompanyRepository>();

        // Act
        var results = await sut.GetConnectorCreationCompanyDataAsync(parameter).ToListAsync();

        // Assert
        results.Should().ContainSingle();
        results.All(x => x.BusinessPartnerNumber is null).Should().BeTrue();
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
