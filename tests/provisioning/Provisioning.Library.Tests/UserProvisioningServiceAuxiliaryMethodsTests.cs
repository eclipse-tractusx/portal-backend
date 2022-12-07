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
using AutoFixture.Dsl;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using FakeItEasy;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests;

public class UserProvisioningServiceAuxiliaryMethodsTests
{
    private readonly IFixture _fixture;
    private Guid _identityProviderId;
    private string _iamUserId;
    private ICustomizationComposer<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, (string? IdpAlias, bool IsSharedIdp) IdentityProvider)> _resultComposer;
    private ICustomizationComposer<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, IEnumerable<string> IdpAliase)> _sharedIdpComposer;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _identityProviderRepository;

    public UserProvisioningServiceAuxiliaryMethodsTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _identityProviderId = _fixture.Create<Guid>();
        _iamUserId = _fixture.Create<string>();
        _resultComposer = _fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, (string? IdpAlias, bool IsSharedIdp) IdentityProvider)>();
        _sharedIdpComposer = _fixture.Build<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company, (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser, IEnumerable<string> IdpAliase)>();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._,A<string>._)).Returns(
            _resultComposer.Create());

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._,A<Guid?>._,A<IdentityProviderCategoryId>._)).Returns(
            _sharedIdpComposer.With(x => x.IdpAliase, new [] { _fixture.Create<string>() }).Create());
    }

    #region GetCompanyNameIdpAliasData

    [Fact]
    public async void TestCompanyNameIdpAliasDataFixtureSetup()
    {
        var sut = new UserProvisioningService(null!,_portalRepositories);

        var result = await sut.GetCompanyNameIdpAliasData(_identityProviderId,_iamUserId).ConfigureAwait(false);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).MustHaveHappened();
        result.Should().NotBeNull();
    }

    [Fact]
    public async void TestCompanyNameIdpAliasDataNotFound()
    {
        ((Guid, string?, string?), (Guid, string?, string?, string?), (string?, bool)) notfound = default;

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._,A<string>._))
            .Returns(notfound);

        var sut = new UserProvisioningService(null!,_portalRepositories);

        async Task Act() => await sut.GetCompanyNameIdpAliasData(_identityProviderId,_iamUserId).ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().Be($"user {_iamUserId} is not associated with any company");
    }

    [Fact]
    public async void TestCompanyNameIdpAliasDataIdpAliasNullThrows()
    {
        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._,A<string>._))
            .Returns(_resultComposer.With(
                x => x.IdentityProvider,
                    _fixture.Build<(string? IdpAlias, bool IsSharedIdp)>()
                        .With(x => x.IdpAlias, (string?)null)
                        .Create())
                .Create());

        var sut = new UserProvisioningService(null!,_portalRepositories);

        Task Act() => sut.GetCompanyNameIdpAliasData(_identityProviderId,_iamUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().Be($"user {_iamUserId} is not associated with own idp {_identityProviderId}");
    }

    [Fact]
    public async void TestCompanyNameIdpAliasDataCompanyNameNullThrows()
    {
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliasUntrackedAsync(A<Guid>._,A<string>._))
            .Returns(_resultComposer.With(
                x => x.Company,
                    _fixture.Build<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber)>()
                        .With(x => x.CompanyName, (string?)null)
                        .With(x => x.CompanyId, companyId)
                        .Create())
                .Create());

        var sut = new UserProvisioningService(null!,_portalRepositories);

        Task Act() => sut.GetCompanyNameIdpAliasData(_identityProviderId,_iamUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"assertion failed: companyName of company {companyId} should never be null here");
    }

    #endregion

    #region GetCompanyNameIdpAliasData

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataFixtureSetup()
    {
        var sut = new UserProvisioningService(null!,_portalRepositories);

        var result = await sut.GetCompanyNameSharedIdpAliasData(_iamUserId).ConfigureAwait(false);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).MustHaveHappened();
        result.Should().NotBeNull();
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataNotFound()
    {
        ((Guid, string?, string?), (Guid, string?, string?, string?), IEnumerable<string>) notfound = default;

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._,A<Guid?>._,A<IdentityProviderCategoryId>._))
            .Returns(notfound);

        var sut = new UserProvisioningService(null!,_portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_iamUserId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user {_iamUserId} is not associated with any company");
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataForApplicationIdNotFound()
    {
        ((Guid, string?, string?), (Guid, string?, string?, string?), IEnumerable<string>) notfound = default;

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._,A<Guid?>._,A<IdentityProviderCategoryId>._))
            .Returns(notfound);

        var applicationId = _fixture.Create<Guid>();

        var sut = new UserProvisioningService(null!,_portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_iamUserId, applicationId);

        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user {_iamUserId} is not associated with application {applicationId}");
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataNoIdpAliasThrows()
    {
        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._,A<Guid?>._,A<IdentityProviderCategoryId>._))
            .Returns(_sharedIdpComposer.With(x => x.IdpAliase, Enumerable.Empty<string>()).Create());

        var sut = new UserProvisioningService(null!,_portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_iamUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user {_iamUserId} is not associated with any shared idp");
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataMultipleIdpAliaseThrows()
    {
        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._,A<Guid?>._,A<IdentityProviderCategoryId>._))
            .Returns(_sharedIdpComposer.With(x => x.IdpAliase, _fixture.CreateMany<string>(2)).Create());

        var sut = new UserProvisioningService(null!,_portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_iamUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"user {_iamUserId} is associated with more than one shared idp");
    }

    [Fact]
    public async void TestGetCompanyNameSharedIdpAliasDataCompanyNameNullThrows()
    {
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _identityProviderRepository.GetCompanyNameIdpAliaseUntrackedAsync(A<string>._,A<Guid?>._,A<IdentityProviderCategoryId>._))
            .Returns(_sharedIdpComposer
                .With(x => x.Company,
                    _fixture.Build<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber)>()
                        .With(x => x.CompanyName, (string?)null)
                        .With(x => x.CompanyId, companyId)
                        .Create())
                .Create());

        var sut = new UserProvisioningService(null!,_portalRepositories);

        Task Act() => sut.GetCompanyNameSharedIdpAliasData(_iamUserId);

        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"assertion failed: companyName of company {companyId} should never be null here");
    }

    #endregion
}
