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

using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BpdmBusinessLogicTests
{
    #region Initialization

    private static readonly Guid IdWithBpn = new ("c244f79a-7faf-4c59-bb85-fbfdf72ce46f");
    private static readonly Guid IdWithStateCreated = new ("bda6d1b5-042e-493a-894c-11f3a89c12b1");
    private static readonly Guid IdWithoutZipCode = new ("beaa6de5-d411-4da8-850e-06047d3170be");
    private static readonly string IamUserId = new Guid("4C1A6851-D4E7-4E10-A011-3732CD045E8A").ToString();
    private const string ValidCompanyName = "valid company";

    private readonly IFixture _fixture;
    private readonly ICompanyRepository _companyRepository;
    
    private readonly IBpdmService _bpdmService;
    
    private readonly BpdmBusinessLogic _logic;

    public BpdmBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization {ConfigureMembers = true});
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var portalRepository = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _bpdmService = A.Fake<IBpdmService>();

        A.CallTo(() => portalRepository.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _logic = new BpdmBusinessLogic(portalRepository, _bpdmService);
    }

    #endregion

    #region Trigger BpnDataPush

    [Fact]
    public async Task TriggerBpnDataPush_WithoutExistingApplication_ThrowsNotFoundException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _logic.TriggerBpnDataPush(applicationId, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Application {applicationId} does not exists.");
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithInvalidUser_ThrowsForbiddenException()
    {
        // Arrange
        var user = Guid.NewGuid().ToString();
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _logic.TriggerBpnDataPush(IdWithBpn, user, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"User is not allowed to trigger Bpn Data Push for the application {IdWithBpn}");
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithCreatedApplication_ThrowsArgumentException()
    {
        // Arrange
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _logic.TriggerBpnDataPush(IdWithStateCreated, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        ex.Message.Should().Be($"CompanyApplication {IdWithStateCreated} is not in status SUBMITTED (Parameter 'applicationId')");
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithEmptyZipCode_ThrowsConflictException()
    {
        // Arrange
        SetupFakesForTrigger();

        // Act
        async Task Act() => await _logic.TriggerBpnDataPush(IdWithoutZipCode, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("ZipCode must not be empty");
    }

    [Fact]
    public async Task TriggerBpnDataPush_WithValidData_CallsExpected()
    {
        // Arrange
        SetupFakesForTrigger();

        // Act
        var result = await _logic.TriggerBpnDataPush(IdWithBpn, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        A.CallTo(() => _bpdmService.TriggerBpnDataPush(
                A<BpdmTransferData>.That.Matches(x => x.ZipCode == "50668" && x.CompanyName == ValidCompanyName),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup
    
    private void SetupFakesForTrigger()
    {
        var validData = _fixture.Build<BpdmData>()
            .With(x => x.ApplicationStatusId, CompanyApplicationStatusId.SUBMITTED)
            .With(x => x.IsUserInCompany, true)
            .With(x => x.ZipCode, "50668")
            .With(x => x.CompanyName, ValidCompanyName)
            .Create();

        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(A<string>.That.Not.Matches(x => x == IamUserId), IdWithBpn))
            .ReturnsLazily(() => new BpdmData(CompanyApplicationStatusId.SUBMITTED, null!, null!, null!, null!, null!, false));
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, A<Guid>.That.Matches(x => x == IdWithBpn)))
            .ReturnsLazily(() => validData);
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, A<Guid>.That.Matches(x => x == IdWithStateCreated)))
            .ReturnsLazily(() => new BpdmData(CompanyApplicationStatusId.CREATED, ValidCompanyName, null!, null!, null!, null!, true));
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, A<Guid>.That.Matches(x => x == IdWithoutZipCode)))
            .ReturnsLazily(() => new BpdmData(CompanyApplicationStatusId.SUBMITTED, ValidCompanyName, null!, null!, null!, null!, true));
        A.CallTo(() => _companyRepository.GetBpdmDataForApplicationAsync(IamUserId, A<Guid>.That.Not.Matches(x => x == IdWithStateCreated || x == IdWithBpn || x == IdWithoutZipCode)))
            .ReturnsLazily(() => (BpdmData?)null);

        A.CallTo(() => _bpdmService.TriggerBpnDataPush(
                A<BpdmTransferData>.That.Matches(x => x.CompanyName == ValidCompanyName && x.ZipCode == "50668"),
                A<CancellationToken>._))
            .ReturnsLazily(() => true);
    }

    #endregion
}
