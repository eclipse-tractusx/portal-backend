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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

public class NetworkRepositoryTests
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly Guid _validCompanyId = new("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87");

    public NetworkRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateNetworkRegistration

    [Fact]
    public async Task CreateNetworkRegistration_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();
        var externalId = Guid.NewGuid().ToString();
        var processId = new Guid("0cc208c3-bdf6-456c-af81-6c3ebe14fe07");
        var ospId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();

        // Act
        var results = sut.CreateNetworkRegistration(externalId, _validCompanyId, processId, ospId, applicationId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.CompanyId.Should().Be(_validCompanyId);
        results.ProcessId.Should().Be(processId);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<NetworkRegistration>()
            .Which.Should().Match<NetworkRegistration>(x =>
                x.CompanyId == _validCompanyId &&
                x.ProcessId == processId
            );
    }

    #endregion

    #region CheckExternalIdExists

    [Fact]
    public async Task CheckExternalIdExists_WithValid_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckExternalIdExists(new Guid("c5547c9a-6ace-4ab7-9253-af65a66278f2").ToString(), new Guid("ac861325-bc54-4583-bcdc-9e9f2a38ff84"));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckExternalIdExists_WithOtherOsp_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckExternalIdExists(new Guid("c5547c9a-6ace-4ab7-9253-af65a66278f2").ToString(), _validCompanyId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckExternalIdExists_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckExternalIdExists(Guid.NewGuid().ToString(), new Guid("ac861325-bc54-4583-bcdc-9e9f2a38ff84"));

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetNetworkRegistrationDataForProcessIdAsync

    [Fact]
    public async Task GetNetworkRegistrationDataForProcessIdAsync_WithValid_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetNetworkRegistrationDataForProcessIdAsync(new Guid("0cc208c3-bdf6-456c-af81-6c3ebe14fe07"));

        // Assert
        result.Should().Be(new Guid("67ace0a9-b6df-438b-935a-fe858b8598dd"));
    }

    #endregion

    #region IsValidRegistration

    [Fact]
    public async Task IsValidRegistration_WithValid_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.IsValidRegistration(new Guid("c5547c9a-6ace-4ab7-9253-af65a66278f2").ToString(), Enumerable.Repeat(ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER, 1));

        // Assert
        result.RegistrationIdExists.Should().BeTrue();
        result.processData.Process.Should().NotBeNull();
        result.processData.Process!.Id.Should().Be(new Guid("0cc208c3-bdf6-456c-af81-6c3ebe14fe07"));
        result.processData.ProcessSteps.Should().ContainSingle()
            .Which.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
    }

    #endregion

    #region GetSubmitData

    [Fact]
    public async Task GetSubmitData_WithoutNetworkRegistration_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSubmitData(new Guid("3390c2d7-75c1-4169-aa27-6ce00e1f3cdd"));

        // Assert
        result.Exists.Should().BeTrue();
        result.CompanyApplications.Should().BeEmpty();
        result.CompanyRoleAgreementIds.Should().Satisfy(x => x.CompanyRoleId == CompanyRoleId.SERVICE_PROVIDER && x.AgreementIds.Count() == 3);
        result.ProcessId.Should().BeNull();
    }

    [Fact]
    public async Task GetSubmitData_WithValid_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSubmitData(new Guid("ac861325-bc54-4583-bcdc-9e9f2a38ff84"));

        // Assert
        result.Should().NotBe(default);
        result.Exists.Should().BeTrue();
        result.CompanyRoleAgreementIds.Should().HaveCount(2).And.Satisfy(
            x => x.CompanyRoleId == CompanyRoleId.ACTIVE_PARTICIPANT,
            x => x.CompanyRoleId == CompanyRoleId.APP_PROVIDER);
        result.CompanyApplications.Should().BeEmpty();
        result.ProcessId.Should().Be("0cc208c3-bdf6-456c-af81-6c3ebe14fe07");
    }

    #endregion

    #region GetCallbackData

    [Fact]
    public async Task GetCallbackData_WithValid_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCallbackData(new Guid("67ace0a9-b6df-438b-935a-fe858b8598dd"), ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED);

        // Assert
        result.Should().NotBe(default);
        result.OspDetails.Should().BeNull();
        result.ExternalId.Should().Be(new Guid("c5547c9a-6ace-4ab7-9253-af65a66278f2").ToString());
        result.ApplicationId.Should().Be(new Guid("7f31e08c-4420-4eac-beab-9540fbd55595"));
        result.Comments.Should().BeEmpty();
        result.Bpn.Should().Be("BPNL00000003AYRE");
    }

    [Fact]
    public async Task GetCallbackData_WithInValid_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCallbackData(new Guid("deadbeef-dead-beef-dead-beefdeadbeef"), ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED);

        // Assert
        result.Should().Be(default);
        result.OspDetails.Should().BeNull();
        result.ExternalId.Should().BeNull();
        result.ApplicationId.Should().Be(Guid.Empty);
        result.Comments.Should().BeNull();
        result.Bpn.Should().BeNull();
    }

    #endregion

    #region Setup

    private async Task<(NetworkRepository sut, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new NetworkRepository(context);
        return (sut, context);
    }

    private async Task<NetworkRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext();
        var sut = new NetworkRepository(context);
        return sut;
    }

    #endregion
}
