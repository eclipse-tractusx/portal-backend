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

using AutoFixture.Dsl;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class UserProvisioningServiceAuxiliaryMethodsTests
{
    private readonly IFixture _fixture;
    private readonly Guid _identityProviderId;
    private readonly Guid _companyUserId;
    private readonly ICustomizationComposer<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, (string? IdpAlias, bool IsSharedIdp) IdentityProvider)> _resultComposer;
    private readonly ICustomizationComposer<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, IEnumerable<(Guid Id, string Alias)> IdpAliase)> _sharedIdpComposer;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _identityProviderRepository;

    public UserProvisioningServiceAuxiliaryMethodsTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _identityProviderId = _fixture.Create<Guid>();
        _companyUserId = Guid.NewGuid();
        _resultComposer = _fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, (string? IdpAlias, bool IsSharedIdp) IdentityProvider)>();
        _sharedIdpComposer = _fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, IEnumerable<(Guid Id, string Alias)> IdpAliase)>();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._, A<Guid>._)).Returns(
            _resultComposer.Create());

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<Guid>._, A<Guid?>._, A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._)).Returns(
            _sharedIdpComposer.With(x => x.IdpAliase, new[] { _fixture.Create<ValueTuple<Guid, string>>() }).Create());
    }

    #region GetCompanyNameIdpAliasData

    [Fact]
    public async void TestCompanyNameIdpAliasDataFixtureSetup()
    {
        var sut = new UserProvisioningService(null!, _portalRepositories);

        var result = await sut.GetCompanyNameIdpAliasData(_identityProviderId, _companyUserId).ConfigureAwait(false);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).MustHaveHappened();
        result.Should().NotBeNull();
    }

    [Fact]
    public async void TestCompanyNameIdpAliaswithNullCompanyNameAndEmailDataFixtureSetup()
    {
        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._, A<Guid>._))
            .Returns(_resultComposer.With(
                x => x.CompanyUser,
                    _fixture.Build<(Guid CompanyUserId, string? FirstName, string? LastName, string? Email)>()
                        .With(x => x.FirstName, (string?)null)
                        .With(x => x.LastName, (string?)null)
                        .With(x => x.Email, (string?)null)
                        .Create())
                .Create());
        var sut = new UserProvisioningService(null!, _portalRepositories);

        var result = await sut.GetCompanyNameIdpAliasData(_identityProviderId, _companyUserId).ConfigureAwait(false);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).MustHaveHappened();
        result.Should().NotBeNull();
        result.NameCreatedBy.Should().Be("Dear User");
    }

    [Fact]
    public async void TestCompanyNameIdpAliasDataNotFound()
    {
        ((Guid, string?, string?), (Guid, string?, string?, string?), (string?, bool)) notfound = default;

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._, A<Guid>._))
            .Returns(notfound);

        var sut = new UserProvisioningService(null!, _portalRepositories);

        async Task Act() => await sut.GetCompanyNameIdpAliasData(_identityProviderId, _companyUserId).ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().Be($"user {_companyUserId} does not exist");
    }

    [Fact]
    public async void TestCompanyNameIdpAliasDataIdpAliasNullThrows()
    {
        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._, A<Guid>._))
            .Returns(_resultComposer.With(
                x => x.IdentityProvider,
                    _fixture.Build<(string? IdpAlias, bool IsSharedIdp)>()
                        .With(x => x.IdpAlias, (string?)null)
                        .Create())
                .Create());

        var sut = new UserProvisioningService(null!, _portalRepositories);

        Task Act() => sut.GetCompanyNameIdpAliasData(_identityProviderId, _companyUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().Be($"user {_companyUserId} is not associated with own idp {_identityProviderId}");
    }

    [Fact]
    public async void TestCompanyNameIdpAliasDataCompanyNameNullThrows()
    {
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._, A<Guid>._))
            .Returns(_resultComposer.With(
                x => x.Company,
                    _fixture.Build<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber)>()
                        .With(x => x.CompanyName, (string?)null)
                        .With(x => x.CompanyId, companyId)
                        .Create())
                .Create());

        var sut = new UserProvisioningService(null!, _portalRepositories);

        Task Act() => sut.GetCompanyNameIdpAliasData(_identityProviderId, _companyUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"assertion failed: companyName of company {companyId} should never be null here");
    }

    #endregion

    #region GetCompanyNameIdpAliasData

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataFixtureSetup()
    {
        var sut = new UserProvisioningService(null!, _portalRepositories);

        var result = await sut.GetCompanyNameSharedIdpAliasData(_companyUserId).ConfigureAwait(false);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).MustHaveHappened();
        result.Should().NotBeNull();
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpwithNullCompanyNameAndEmailAliasDataFixtureSetup()
    {
        // Arrange
        var data = new ValueTuple<(Guid, string?, string?), (Guid, string?, string?, string?), IEnumerable<(Guid Id, string Alias)>>(
            new ValueTuple<Guid, string?, string?>(Guid.NewGuid(), _fixture.Create<string>(), _fixture.Create<string>()),
            new ValueTuple<Guid, string?, string?, string?>(Guid.NewGuid(), null, null, null),
            _fixture.CreateMany<ValueTuple<Guid, string>>(1)

        );

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<Guid>._, A<Guid?>._, A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._))
            .Returns(data);

        // Act
        var sut = new UserProvisioningService(null!, _portalRepositories);

        // Assert
        var result = await sut.GetCompanyNameSharedIdpAliasData(_companyUserId).ConfigureAwait(false);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).MustHaveHappened();
        result.Should().NotBeNull();
        result.NameCreatedBy.Should().Be("Dear User");
    }
    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataNotFound()
    {
        ((Guid, string?, string?), (Guid, string?, string?, string?), IEnumerable<(Guid Id, string Alias)>) notfound = default;

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<Guid>._, A<Guid?>._, A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._))
            .Returns(notfound);

        var sut = new UserProvisioningService(null!, _portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_companyUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user {_companyUserId} does not exist");
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataForApplicationIdNotFound()
    {
        ((Guid, string?, string?), (Guid, string?, string?, string?), IEnumerable<(Guid Id, string Alias)>) notfound = default;

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<Guid>._, A<Guid?>._, A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._))
            .Returns(notfound);

        var applicationId = _fixture.Create<Guid>();

        var sut = new UserProvisioningService(null!, _portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_companyUserId, applicationId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user {_companyUserId} is not associated with application {applicationId}");
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataNoIdpAliasThrows()
    {
        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<Guid>._, A<Guid?>._, A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._))
            .Returns(_sharedIdpComposer.With(x => x.IdpAliase, Enumerable.Empty<(Guid Id, string Alias)>()).Create());

        var sut = new UserProvisioningService(null!, _portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_companyUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user {_companyUserId} is not associated with any shared idp");
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataMultipleIdpAliaseThrows()
    {
        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<Guid>._, A<Guid?>._, A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._))
            .Returns(_sharedIdpComposer.With(x => x.IdpAliase, _fixture.CreateMany<(Guid Id, string Alias)>(2)).Create());

        var sut = new UserProvisioningService(null!, _portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_companyUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user {_companyUserId} is associated with more than one shared idp");
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataCompanyNameNullThrows()
    {
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<Guid>._, A<Guid?>._, A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._))
            .Returns(_sharedIdpComposer
                .With(x => x.Company,
                    _fixture.Build<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber)>()
                        .With(x => x.CompanyName, (string?)null)
                        .With(x => x.CompanyId, companyId)
                        .Create())
                .Create());

        var sut = new UserProvisioningService(null!, _portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_companyUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"assertion failed: companyName of company {companyId} should never be null here");
    }

    #endregion
}
