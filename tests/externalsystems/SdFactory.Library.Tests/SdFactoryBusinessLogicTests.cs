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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Tests;

public class SdFactoryBusinessLogicTests
{
    #region Initialization
    
    private const string CountryCode = "DE";
    private const string Bpn = "BPNL000000000009";
    private static readonly Guid ApplicationId = new("ac1cf001-7fbc-1f2f-817f-bce058020001");
    private static readonly Guid CompanyId = new("b4697623-dd87-410d-abb8-6d4f4d87ab58");
    private static readonly IEnumerable<(UniqueIdentifierId Id, string Value)> UniqueIdentifiers = new List<(UniqueIdentifierId Id, string Value)>
    {
        new (UniqueIdentifierId.VAT_ID, "JUSTATEST")
    };

    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ISdFactoryService _service;

    private readonly SdFactoryBusinessLogic _sut;
    private readonly IFixture _fixture;

    public SdFactoryBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _applicationRepository = A.Fake<IApplicationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _service = A.Fake<ISdFactoryService>();

        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>()).Returns(_applicationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _sut = new SdFactoryBusinessLogic(_service, _portalRepositories);
    }

    #endregion
    
    #region Register Connector
    
    [Fact]
    public async Task RegisterConnectorAsync_ExpectedServiceCallIsMade()
    {
        // Arrange
        const string url = "https://connect-tor.com";

        // Act
        await _sut.RegisterConnectorAsync(url, Bpn, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _service.RegisterConnectorAsync(url, Bpn, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region RegisterSelfDescription
    
    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithValidData_CompanyIsUpdated()
    {
        // Arrange
        var entity = _fixture.Build<Company>()
            .With(x => x.SelfDescriptionDocumentId, (Guid?)null)
            .Create();
        var documentId = Guid.NewGuid();
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .ReturnsLazily(() => new ValueTuple<Guid, string?, string, IEnumerable<(UniqueIdentifierId Id, string Value)>>(CompanyId, Bpn, CountryCode, UniqueIdentifiers));
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(UniqueIdentifiers, CountryCode, Bpn, CancellationToken.None))
            .ReturnsLazily(() => documentId);
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(CompanyId, null, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? initialize, Action<Company> modify) => 
            {
                initialize?.Invoke(entity);
                modify.Invoke(entity);
            });

        // Act
        await _sut.RegisterSelfDescriptionAsync(ApplicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _service.RegisterSelfDescriptionAsync(UniqueIdentifiers, CountryCode, Bpn, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustNotHaveHappened();
        entity.SelfDescriptionDocumentId.Should().NotBeNull().And.Be(documentId);
    }

    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithNoApplication_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .ReturnsLazily(() => new ValueTuple<Guid, string?, string, IEnumerable<(UniqueIdentifierId Id, string Value)>>());

        // Act
        async Task Act() => await _sut.RegisterSelfDescriptionAsync(ApplicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"CompanyApplication {ApplicationId} is not in status SUBMITTED");
    }

    [Fact]
    public async Task RegisterSelfDescriptionAsync_WithBpnNotSet_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _applicationRepository.GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(ApplicationId))
            .ReturnsLazily(() => new ValueTuple<Guid, string?, string, IEnumerable<(UniqueIdentifierId Id, string Value)>>(CompanyId, null, CountryCode, new List<(UniqueIdentifierId Id, string Value)>()));

        // Act
        async Task Act() => await _sut.RegisterSelfDescriptionAsync(ApplicationId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"BusinessPartnerNumber (bpn) for CompanyApplications {ApplicationId} company {CompanyId} is empty");
    }

    #endregion
    
    #region GetSdUniqueIdentifierValue

    [Theory]
    [InlineData(UniqueIdentifierId.COMMERCIAL_REG_NUMBER, "local")]
    [InlineData(UniqueIdentifierId.VAT_ID, "vatID")]
    [InlineData(UniqueIdentifierId.LEI_CODE, "leiCode")]
    [InlineData(UniqueIdentifierId.VIES, "EUID")]
    [InlineData(UniqueIdentifierId.EORI, "EORI")]
    public void GetSdUniqueIdentifierValue_WithIdentifier_ReturnsExpected(UniqueIdentifierId uiId, string expectedValue)
    {
        // Act
        var result = uiId.GetSdUniqueIdentifierValue();
        
        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetSdUniqueIdentifierValue_WithNotYetConfiguredValue_ThrowsArgumentOutOfRangeException()
    {
        // Assert
        UniqueIdentifierId id = default;
        
        // Act
        Func<string> Act = () => id.GetSdUniqueIdentifierValue();
        
        // Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(Act);
        ex.ParamName.Should().Be("uniqueIdentifierId");
    }

    #endregion
}
