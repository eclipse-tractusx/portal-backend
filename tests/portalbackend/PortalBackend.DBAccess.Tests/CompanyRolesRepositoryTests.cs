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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="CompanyRepository"/>
/// </summary>
public class CompanyRolesRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public CompanyRolesRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetCompanyRoleAndConsentAgreement

    [Fact]
    public async Task GetCompanyRoleAndConsentAgreementDetailsAsync_ReturnsExpected()
    {
        var (sut, _) = await CreateSut();
        var applicationId = new Guid("7e279133-2148-48b4-b855-9d7d291ecbb1");
        var companyId = new Guid("0dcd8209-85e2-4073-b130-ac094fb47106");

        var result = await sut.GetCompanyRoleAgreementConsentStatusUntrackedAsync(applicationId, companyId);

        result.Should().NotBeNull()
            .And.Match<CompanyRoleAgreementConsents>(x =>
                x.CompanyRoleIds.Count() == 2 &&
                x.CompanyRoleIds.Contains(CompanyRoleId.APP_PROVIDER) &&
                x.CompanyRoleIds.Contains(CompanyRoleId.ACTIVE_PARTICIPANT) &&
                x.AgreementConsentStatuses.Count() == 1 &&
                x.AgreementConsentStatuses.First().AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1018") &&
                x.AgreementConsentStatuses.First().ConsentStatusId == ConsentStatusId.ACTIVE
            );
    }

    #endregion

    #region GetAgreementAssignedCompanyRoles

    [Fact]
    public async Task GetAgreementAssignedCompanyRolesUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var roles = new[] { CompanyRoleId.APP_PROVIDER };
        var agreementStatusData = new[] {
            new AgreementStatusData(new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1011"), AgreementStatusId.ACTIVE)
        };
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetAgreementAssignedCompanyRolesUntrackedAsync(roles).ToListAsync();

        // Assert
        result.Should().NotBeNull()
            .And.HaveCount(1)
            .And.Satisfy(
                x => x.AgreementStatusData.Count() == 1
                && x.AgreementStatusData.SequenceEqual(agreementStatusData)
                && x.CompanyRoleId == CompanyRoleId.APP_PROVIDER
            );
    }

    #endregion

    #region GetCompanyRoleAgreements

    [Fact]
    public async Task GetCompanyRoleAgreementsUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var activeAgreementIds = new[] { new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1010"), new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1013") };
        var appAgreementIds = new[] { new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1011") };
        var serviceAgreementIds = new[] { new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1017"), new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1018"), new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1094") };
        var onboardingAgreementIds = new[] { new Guid("311aac58-932b-4622-be8b-34337e48d70d") };
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetCompanyRoleAgreementsUntrackedAsync().ToListAsync();

        // Assert
        result.Should().NotBeNull().And.HaveCount(4).And.Satisfy(
            x => x.CompanyRoleId == CompanyRoleId.ACTIVE_PARTICIPANT && x.AgreementIds.Count() == 2 &&
                x.AgreementIds.SequenceEqual(activeAgreementIds),
            x => x.CompanyRoleId == CompanyRoleId.APP_PROVIDER && x.AgreementIds.Count() == 1 &&
                x.AgreementIds.SequenceEqual(appAgreementIds),
            x => x.CompanyRoleId == CompanyRoleId.SERVICE_PROVIDER && x.AgreementIds.Count() == 3 &&
                x.AgreementIds.SequenceEqual(serviceAgreementIds),
            x => x.CompanyRoleId == CompanyRoleId.ONBOARDING_SERVICE_PROVIDER && x.AgreementIds.Count() == 1 &&
                x.AgreementIds.SequenceEqual(onboardingAgreementIds)
        );
    }

    #endregion

    #region GetCompanyRoleAgreementConsentStatus

    [Fact]
    public async Task GetCompanyRoleAgreementConsentStatusUntrackedAsync_ReturnsExpected()
    {
        // Arrange
        var applicationId = new Guid("7e279133-2148-48b4-b855-9d7d291ecbb1");
        var companyId = new Guid("0dcd8209-85e2-4073-b130-ac094fb47106");
        var (sut, _) = await CreateSut();

        // Act
        var result = await sut.GetCompanyRoleAgreementConsentStatusUntrackedAsync(applicationId, companyId);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyRoleIds.Should().HaveCount(2);
        result.AgreementConsentStatuses.Should().HaveCount(1).And.Satisfy(
            x => x.AgreementId == new Guid("aa0a0000-7fbc-1f2f-817f-bce0502c1018")
                && x.ConsentStatusId == ConsentStatusId.ACTIVE);
    }

    #endregion

    #region Setup

    private async Task<(ICompanyRolesRepository, PortalDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new CompanyRolesRepository(context);
        return (sut, context);
    }

    #endregion
}
