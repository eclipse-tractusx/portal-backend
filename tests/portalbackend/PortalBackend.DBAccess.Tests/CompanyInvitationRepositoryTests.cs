/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class CompanyInvitationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _processId = new("70f0f368-5058-4aca-808b-cece869bcef2");
    private readonly Guid _invitationId = new("32705785-b056-4f36-9a71-71b795344bb2");

    public CompanyInvitationRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateCompanyInvitation

    [Fact]
    public async Task CreateCompanyInvitation_WithValidData_Creates()
    {
        var (sut, context) = await CreateSutWithContext();
        var processId = Guid.NewGuid();

        var invitation = sut.CreateCompanyInvitation("tony", "stark", "tony@stark.com", "stark industry", processId, x =>
            {
                x.UserName = "ironman";
            });

        // Assert
        invitation.Id.Should().NotBeEmpty();
        var changeTracker = context.ChangeTracker;
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanyInvitation>()
            .Which.UserName.Should().Be("ironman");
    }

    #endregion

    #region GetCompanyInvitationForProcessId

    [Fact]
    public async Task GetCompanyInvitationForProcessId_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetCompanyInvitationForProcessId(_processId);

        // Assert
        data.Should().Be(_invitationId);
    }

    [Fact]
    public async Task GetCompanyInvitationForProcessId_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetCompanyInvitationForProcessId(Guid.NewGuid());

        // Assert
        data.Should().Be(Guid.Empty);
    }

    #endregion

    #region GetOrganisationNameForInvitation

    [Fact]
    public async Task GetOrganisationNameForInvitation_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetOrganisationNameForInvitation(_invitationId);

        // Assert
        data.Should().Be("stark industry");
    }

    [Fact]
    public async Task GetOrganisationNameForInvitation_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetOrganisationNameForInvitation(Guid.NewGuid());

        // Assert
        data.Should().BeNull();
    }

    #endregion

    #region GetInvitationUserData

    [Fact]
    public async Task GetInvitationUserData_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetInvitationUserData(_invitationId);

        // Assert
        data.Exists.Should().BeTrue();
        data.UserInformation.Email.Should().Be("tony@stark.com");
        data.UserInformation.FirstName.Should().Be("tony");
    }

    [Fact]
    public async Task GetInvitationUserData_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetInvitationUserData(Guid.NewGuid());

        // Assert
        data.Exists.Should().BeFalse();
    }

    #endregion

    #region AttachAndModifyCompanyInvitation

    [Fact]
    public async Task AttachAndModifyCompanyInvitation()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();
        var existingId = Guid.NewGuid();

        // Act
        sut.AttachAndModifyCompanyInvitation(existingId, invitation => { invitation.IdpName = null; }, invitation => { invitation.IdpName = "test"; });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle().And.AllSatisfy(x => x.Entity.Should().BeOfType<CompanyInvitation>()).And.Satisfy(
            x => x.State == EntityState.Modified && ((CompanyInvitation)x.Entity).Id == existingId && ((CompanyInvitation)x.Entity).IdpName == "test"
        );
    }

    #endregion

    #region GetIdpNameForInvitationId

    [Fact]
    public async Task GetIdpNameForInvitationId_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetIdpNameForInvitationId(_invitationId);

        // Assert
        data.Should().Be("test idp");
    }

    [Fact]
    public async Task GetIdpNameForInvitationId_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetIdpNameForInvitationId(Guid.NewGuid());

        // Assert
        data.Should().BeNull();
    }

    #endregion

    #region GetUpdateCentralIdpUrlData

    [Fact]
    public async Task GetUpdateCentralIdpUrlData_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetUpdateCentralIdpUrlData(_invitationId);

        // Assert
        data.OrgName.Should().Be("stark industry");
        data.IdpName.Should().Be("test idp");
        data.ClientId.Should().Be("cl1");
    }

    #endregion

    #region GetIdpAndOrgName

    [Fact]
    public async Task GetIdpAndOrgNameAsync_WithExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetIdpAndOrgName(_invitationId);

        // Assert
        data.Exists.Should().BeTrue();
        data.OrgName.Should().Be("stark industry");
        data.IdpName.Should().Be("test idp");
    }

    [Fact]
    public async Task GetInvitationIdpCreationData_WithoutExistingForProcessId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var data = await sut.GetIdpAndOrgName(Guid.NewGuid());

        // Assert
        data.Exists.Should().BeFalse();
    }

    #endregion

    #region Setup

    private async Task<(ICompanyInvitationRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new CompanyInvitationRepository(context);
        return (sut, context);
    }

    private async Task<ICompanyInvitationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new CompanyInvitationRepository(context);
        return sut;
    }

    #endregion
}
