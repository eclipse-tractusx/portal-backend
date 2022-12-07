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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="CompanyRepository"/>
/// </summary>
public class CompanyRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private const string IamUserId = "3d8142f1-860b-48aa-8c2b-1ccb18699f65";
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    private readonly Guid _validDetailId = new("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122");
    
    public CompanyRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Create ServiceProviderCompanyDetail

    [Fact]
    public async Task CreateServiceProviderCompanyDetail_ReturnsExpectedResult()
    {
        // Arrange
        const string url = "https://service-url.com";
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = sut.CreateProviderCompanyDetail(_validCompanyId, url);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.CompanyId.Should().Be(_validCompanyId);
        results.AutoSetupUrl.Should().Be(url);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<ProviderCompanyDetail>().Which.AutoSetupUrl.Should().Be(url);
    }

    #endregion
    
    #region Create ServiceProviderCompanyDetail

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithNotExistingUser_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(_validDetailId, CompanyRoleId.SERVICE_PROVIDER, IamUserId).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBe(default);
        result.ProviderDetailReturnData.Should().NotBeNull();
        result.ProviderDetailReturnData.Id.Should().Be(_validDetailId);
        result.IsProviderCompany.Should().BeTrue();
        result.IsCompanyUser.Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithNotExistingDetails_ReturnsDefault()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(Guid.NewGuid(), CompanyRoleId.SERVICE_PROVIDER, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    [Fact]
    public async Task GetServiceProviderCompanyDetailAsync_WithNotExistingUser_ReturnsIsCompanyUserFalse()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetProviderCompanyDetailAsync(_validDetailId, CompanyRoleId.SERVICE_PROVIDER, Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.IsProviderCompany.Should().BeTrue();
        result.IsCompanyUser.Should().BeFalse();
    }

    #endregion

    #region Check Company is ServiceProvider and exists for IamUser

    [Fact]
    public async Task CheckCompanyIsServiceProviderAndExistsForIamUser_WithValidData_ReturnsTrue()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync("3d8142f1-860b-48aa-8c2b-1ccb18699f66", CompanyRoleId.SERVICE_PROVIDER);

        // Assert
        results.Should().NotBe(default);
        results.CompanyId.Should().NotBe(Guid.Empty);
        results.IsServiceProviderCompany.Should().BeTrue();
    }
    
    [Fact]
    public async Task CheckCompanyIsServiceProviderAndExistsForIamUser_WithNonServiceProviderCompany_ReturnsFalse()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync("4b8f156e-5dfc-4a58-9384-1efb195c1c34", CompanyRoleId.SERVICE_PROVIDER);

        // Assert
        results.Should().NotBe(default);
        results.CompanyId.Should().NotBe(Guid.Empty);
        results.IsServiceProviderCompany.Should().BeFalse();
    }

    #endregion
    
    #region GetCompanyIdByBpn
    
    [Fact]
    public async Task GetCompanyIdByBpn_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyIdByBpnAsync("CAXSDUMMYCATENAZZ").ConfigureAwait(false);

        // Assert
        results.Should().NotBe(Guid.Empty);
        results.Should().Be("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");
    }
     
    [Fact]
    public async Task GetCompanyIdByBpn_WithNotExistingBpn_ReturnsEmptyGuid()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyIdByBpnAsync("NOTEXISTING").ConfigureAwait(false);

        // Assert
        results.Should().Be(Guid.Empty);
    }

    #endregion

    #region GetCompanyBpnById
    
    [Fact]
    public async Task GetCompanyBpnByIdAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyBpnByIdAsync(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87")).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().Be("CAXSDUMMYCATENAZZ");
    }
     
    [Fact]
    public async Task GetCompanyBpnByIdAsync_WithNotExistingId_ReturnsEmpty()
    {
        // Arrange
        var (sut, _) = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetCompanyBpnByIdAsync(Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        results.Should().BeNull();
    }

    #endregion

    #region AttachAndModifyServiceProviderDetails

    [Fact]
    public async Task AttachAndModifyServiceProviderDetails_ReturnsExpectedResult()
    {
        // Arrange
        const string url = "https://service-url.com/new";
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyProviderCompanyDetails(new Guid("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122"),
            detail => { detail.AutoSetupUrl = url; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<ProviderCompanyDetail>().Which.AutoSetupUrl.Should().Be(url);
    }

    #endregion

    #region AttachAndModifyServiceProviderDetails

    [Fact]
    public async Task CheckServiceProviderDetailsExistsForUser_WithValidIamUserAndDetailId_ReturnsTrue()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckProviderCompanyDetailsExistsForUser("623770c5-cf38-4b9f-9a35-f8b9ae972e2d", new Guid("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122")).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.IsSameCompany.Should().BeTrue();
    }

    [Fact]
    public async Task CheckServiceProviderDetailsExistsForUser_WithNotExistingIamUserAndExistingDetailId_ReturnsFalse()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckProviderCompanyDetailsExistsForUser(Guid.NewGuid().ToString(), new Guid("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122")).ConfigureAwait(false);

        // Assert
        result.Should().NotBe(default);
        result.IsSameCompany.Should().BeFalse();
    }

    [Fact]
    public async Task CheckServiceProviderDetailsExistsForUser_WithExistingIamUserAndNotExistingDetailId_ReturnsFalse()
    {
        // Arrange
        var (sut, context) = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.CheckProviderCompanyDetailsExistsForUser("623770c5-cf38-4b9f-9a35-f8b9ae972e2d", Guid.NewGuid()).ConfigureAwait(false);

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region Setup
    
    private async Task<(CompanyRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new CompanyRepository(context);
        return (sut, context);
    }

    #endregion
}
