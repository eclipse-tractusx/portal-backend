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

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Web.Tests;

public class MandatoryIdentityClaimHandlerTests
{
    private readonly IFixture _fixture;
    private readonly IClaimsIdentityDataBuilder _claimsIdentityDataBuilder;
    private readonly IIdentityRepository _identityRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IMockLogger<MandatoryIdentityClaimHandler> _mockLogger;
    private readonly ILogger<MandatoryIdentityClaimHandler> _logger;

    private readonly Guid _companyUserId;
    private readonly Guid _companyUserCompanyId;
    private readonly Guid _serviceAccountId;
    private readonly Guid _serviceAccountCompanyId;
    private readonly string _clientId;
    private readonly string _subject_company_user;
    private readonly string _subject_service_account;

    public MandatoryIdentityClaimHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _claimsIdentityDataBuilder = new ClaimsIdentityDataBuilder();
        _identityRepository = A.Fake<IIdentityRepository>();
        _serviceAccountRepository = A.Fake<IServiceAccountRepository>();
        _portalRepositories = A.Fake<IPortalRepositories>();

        _companyUserId = Guid.Parse("eceefebe-8f34-4d11-85ef-767786a95a92");
        _companyUserCompanyId = Guid.Parse("0d0de79b-c05d-4153-9c97-7331900880d2");
        _serviceAccountId = Guid.Parse("53472768-6cb5-41b0-9421-5c956e44e8c8");
        _serviceAccountCompanyId = Guid.Parse("a61092d7-1516-4ccf-b922-0308bbb087e3");
        _clientId = "valid_client";
        _subject_company_user = "valid_sub_company_user";
        _subject_service_account = "valid_sub_service_account";

        A.CallTo(() => _portalRepositories.GetInstance<IIdentityRepository>()).Returns(_identityRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);

        A.CallTo(() => _serviceAccountRepository.GetServiceAccountDataByClientId(A<string>._)).Returns(default((Guid, Guid)));
        A.CallTo(() => _identityRepository.GetActiveIdentityDataByUserEntityId(A<string>._)).Returns(default((Guid, IdentityTypeId, Guid)));
        A.CallTo(() => _identityRepository.GetActiveCompanyIdByIdentityId(A<Guid>._)).Returns(Guid.Empty);

        A.CallTo(() => _serviceAccountRepository.GetServiceAccountDataByClientId(_clientId)).Returns((_serviceAccountId, _serviceAccountCompanyId));
        A.CallTo(() => _identityRepository.GetActiveIdentityDataByUserEntityId(_subject_company_user)).Returns((_companyUserId, IdentityTypeId.COMPANY_USER, _companyUserCompanyId));
        A.CallTo(() => _identityRepository.GetActiveIdentityDataByUserEntityId(_subject_service_account)).Returns((_serviceAccountId, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, _serviceAccountCompanyId));
        A.CallTo(() => _identityRepository.GetActiveCompanyIdByIdentityId(_companyUserId)).Returns(_companyUserCompanyId);

        _mockLogger = A.Fake<IMockLogger<MandatoryIdentityClaimHandler>>();
        _logger = new MockLogger<MandatoryIdentityClaimHandler>(_mockLogger);
    }

    [Theory]
    [InlineData("preferred_username", "eceefebe-8f34-4d11-85ef-767786a95a92", PolicyTypeId.ValidIdentity, IClaimsIdentityDataBuilderStatus.Initialized, true, "eceefebe-8f34-4d11-85ef-767786a95a92", IdentityTypeId.COMPANY_USER, "00000000-0000-0000-0000-000000000000")]
    [InlineData("preferred_username", "eceefebe-8f34-4d11-85ef-767786a95a92", PolicyTypeId.CompanyUser, IClaimsIdentityDataBuilderStatus.Initialized, true, "eceefebe-8f34-4d11-85ef-767786a95a92", IdentityTypeId.COMPANY_USER, "00000000-0000-0000-0000-000000000000")]
    [InlineData("preferred_username", "eceefebe-8f34-4d11-85ef-767786a95a92", PolicyTypeId.ServiceAccount, IClaimsIdentityDataBuilderStatus.Initialized, false, "eceefebe-8f34-4d11-85ef-767786a95a92", IdentityTypeId.COMPANY_USER, "00000000-0000-0000-0000-000000000000")]
    [InlineData("preferred_username", "eceefebe-8f34-4d11-85ef-767786a95a92", PolicyTypeId.ValidCompany, IClaimsIdentityDataBuilderStatus.Complete, true, "eceefebe-8f34-4d11-85ef-767786a95a92", IdentityTypeId.COMPANY_USER, "0d0de79b-c05d-4153-9c97-7331900880d2")]
    [InlineData("clientId", "valid_client", PolicyTypeId.ValidIdentity, IClaimsIdentityDataBuilderStatus.Complete, true, "53472768-6cb5-41b0-9421-5c956e44e8c8", IdentityTypeId.COMPANY_SERVICE_ACCOUNT, "a61092d7-1516-4ccf-b922-0308bbb087e3")]
    [InlineData("clientId", "valid_client", PolicyTypeId.CompanyUser, IClaimsIdentityDataBuilderStatus.Complete, false, "53472768-6cb5-41b0-9421-5c956e44e8c8", IdentityTypeId.COMPANY_SERVICE_ACCOUNT, "a61092d7-1516-4ccf-b922-0308bbb087e3")]
    [InlineData("clientId", "valid_client", PolicyTypeId.ServiceAccount, IClaimsIdentityDataBuilderStatus.Complete, true, "53472768-6cb5-41b0-9421-5c956e44e8c8", IdentityTypeId.COMPANY_SERVICE_ACCOUNT, "a61092d7-1516-4ccf-b922-0308bbb087e3")]
    [InlineData("clientId", "valid_client", PolicyTypeId.ValidCompany, IClaimsIdentityDataBuilderStatus.Complete, true, "53472768-6cb5-41b0-9421-5c956e44e8c8", IdentityTypeId.COMPANY_SERVICE_ACCOUNT, "a61092d7-1516-4ccf-b922-0308bbb087e3")]
    [InlineData("sub", "valid_sub_company_user", PolicyTypeId.ValidIdentity, IClaimsIdentityDataBuilderStatus.Complete, true, "eceefebe-8f34-4d11-85ef-767786a95a92", IdentityTypeId.COMPANY_USER, "0d0de79b-c05d-4153-9c97-7331900880d2")]
    [InlineData("sub", "valid_sub_company_user", PolicyTypeId.CompanyUser, IClaimsIdentityDataBuilderStatus.Complete, true, "eceefebe-8f34-4d11-85ef-767786a95a92", IdentityTypeId.COMPANY_USER, "0d0de79b-c05d-4153-9c97-7331900880d2")]
    [InlineData("sub", "valid_sub_company_user", PolicyTypeId.ServiceAccount, IClaimsIdentityDataBuilderStatus.Complete, false, "eceefebe-8f34-4d11-85ef-767786a95a92", IdentityTypeId.COMPANY_USER, "0d0de79b-c05d-4153-9c97-7331900880d2")]
    [InlineData("sub", "valid_sub_company_user", PolicyTypeId.ValidCompany, IClaimsIdentityDataBuilderStatus.Complete, true, "eceefebe-8f34-4d11-85ef-767786a95a92", IdentityTypeId.COMPANY_USER, "0d0de79b-c05d-4153-9c97-7331900880d2")]
    [InlineData("sub", "valid_sub_service_account", PolicyTypeId.ValidIdentity, IClaimsIdentityDataBuilderStatus.Complete, true, "53472768-6cb5-41b0-9421-5c956e44e8c8", IdentityTypeId.COMPANY_SERVICE_ACCOUNT, "a61092d7-1516-4ccf-b922-0308bbb087e3")]
    [InlineData("sub", "valid_sub_service_account", PolicyTypeId.CompanyUser, IClaimsIdentityDataBuilderStatus.Complete, false, "53472768-6cb5-41b0-9421-5c956e44e8c8", IdentityTypeId.COMPANY_SERVICE_ACCOUNT, "a61092d7-1516-4ccf-b922-0308bbb087e3")]
    [InlineData("sub", "valid_sub_service_account", PolicyTypeId.ServiceAccount, IClaimsIdentityDataBuilderStatus.Complete, true, "53472768-6cb5-41b0-9421-5c956e44e8c8", IdentityTypeId.COMPANY_SERVICE_ACCOUNT, "a61092d7-1516-4ccf-b922-0308bbb087e3")]
    [InlineData("sub", "valid_sub_service_account", PolicyTypeId.ValidCompany, IClaimsIdentityDataBuilderStatus.Complete, true, "53472768-6cb5-41b0-9421-5c956e44e8c8", IdentityTypeId.COMPANY_SERVICE_ACCOUNT, "a61092d7-1516-4ccf-b922-0308bbb087e3")]
    [InlineData(null, null, PolicyTypeId.ValidIdentity, IClaimsIdentityDataBuilderStatus.Empty, false, "00000000-0000-0000-0000-000000000000", default(IdentityTypeId), "00000000-0000-0000-0000-000000000000")]
    [InlineData(null, null, PolicyTypeId.CompanyUser, IClaimsIdentityDataBuilderStatus.Empty, false, "00000000-0000-0000-0000-000000000000", default(IdentityTypeId), "00000000-0000-0000-0000-000000000000")]
    [InlineData(null, null, PolicyTypeId.ServiceAccount, IClaimsIdentityDataBuilderStatus.Empty, false, "00000000-0000-0000-0000-000000000000", default(IdentityTypeId), "00000000-0000-0000-0000-000000000000")]
    [InlineData(null, null, PolicyTypeId.ValidCompany, IClaimsIdentityDataBuilderStatus.Empty, false, "00000000-0000-0000-0000-000000000000", default(IdentityTypeId), "00000000-0000-0000-0000-000000000000")]
    public async Task HandleValidRequirement_ReturnsExpected(string? claim, string? value, PolicyTypeId policyType, IClaimsIdentityDataBuilderStatus status, bool success, Guid identityId, IdentityTypeId identityTypeId, Guid companyId)
    {
        // Arrange
        var principal = new ClaimsPrincipal(
            claim == null || value == null
                ? Enumerable.Empty<ClaimsIdentity>()
                : new[] { new ClaimsIdentity(new[] { new Claim(claim, value) }) });

        var context = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(policyType), 1), principal, null);
        var sut = new MandatoryIdentityClaimHandler(_claimsIdentityDataBuilder, _portalRepositories, _logger);

        // Act
        await sut.HandleAsync(context).ConfigureAwait(false);

        // Assert
        context.HasSucceeded.Should().Be(success);
        _claimsIdentityDataBuilder.Status.Should().Be(status);

        if (identityId == Guid.Empty)
        {
            Assert.Throws<UnexpectedConditionException>(() => _claimsIdentityDataBuilder.IdentityId);
        }
        else
        {
            _claimsIdentityDataBuilder.IdentityId.Should().Be(identityId);
        }

        if (identityTypeId == default)
        {
            Assert.Throws<UnexpectedConditionException>(() => _claimsIdentityDataBuilder.IdentityTypeId);
        }
        else
        {
            _claimsIdentityDataBuilder.IdentityTypeId.Should().Be(identityTypeId);
        }

        if (companyId == Guid.Empty)
        {
            Assert.Throws<UnexpectedConditionException>(() => _claimsIdentityDataBuilder.CompanyId);
        }
        else
        {
            _claimsIdentityDataBuilder.CompanyId.Should().Be(companyId);
        }
    }
}
