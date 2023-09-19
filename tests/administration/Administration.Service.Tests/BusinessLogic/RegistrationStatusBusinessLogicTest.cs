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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class RegistrationStatusBusinessLogicTest
{
    private readonly IdentityData _identity = new("4C1A6851-D4E7-4E10-A011-3732CD045E8A", Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());

    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly IRegistrationStatusBusinessLogic _logic;

    public RegistrationStatusBusinessLogicTest()
    {
        var identityService = A.Fake<IIdentityService>();
        A.CallTo(() => identityService.IdentityData).Returns(_identity);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        var options = new OnboardingServiceProviderSettings
        {
            EncryptionKey = "mysmallkey1234551298765134567890"
        };
        _logic = new RegistrationStatusBusinessLogic(_portalRepositories, identityService, Options.Create(options));
    }

    #region GetCallbackAddress

    [Fact]
    public async Task GetCallbackAddress_WithExistingOspData_ReturnsExpectedCallbackUrl()
    {
        //Arrange
        A.CallTo(() => _companyRepository.GetCallbackData(_identity.CompanyId))
            .Returns(new OnboardingServiceProviderCallbackResponseData("https://callback-url.com", "https//auth.url", "test"));

        //Act
        var result = await _logic.GetCallbackAddress().ConfigureAwait(false);

        //Assert
        result.CallbackUrl.Should().Be("https://callback-url.com");
    }

    [Fact]
    public async Task GetCallbackAddress_WithoutOspData_ReturnsNull()
    {
        //Arrange
        A.CallTo(() => _companyRepository.GetCallbackData(_identity.CompanyId))
            .Returns(new OnboardingServiceProviderCallbackResponseData(null, null, null));

        //Act
        var result = await _logic.GetCallbackAddress().ConfigureAwait(false);

        //Assert
        result.CallbackUrl.Should().BeNull();
    }

    #endregion

    #region SetCallbackAddress

    [Fact]
    public async Task SetCallbackAddress_WithCompanyNotOsp_ReturnsExpectedCallbackUrl()
    {
        //Arrange
        A.CallTo(() => _companyRepository.GetCallbackEditData(A<Guid>._, A<CompanyRoleId>._))
            .Returns(((bool, OspDetails?))default);

        //Act
        async Task Act() => await _logic.SetCallbackAddress(new OnboardingServiceProviderCallbackRequestData("https://test.de", "https//auth.url", "test", "Sup3rS3cureTest!")).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Only {CompanyRoleId.ONBOARDING_SERVICE_PROVIDER} are allowed to set the callback url");
        A.CallTo(() => _companyRepository.GetCallbackEditData(_identity.CompanyId, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetCallbackAddress_WithNonExistingOspData_InsertExpected()
    {
        //Arrange
        A.CallTo(() => _companyRepository.GetCallbackEditData(A<Guid>._, A<CompanyRoleId>._))
            .Returns((true, null));

        //Act
        await _logic.SetCallbackAddress(new OnboardingServiceProviderCallbackRequestData("https://test.de", "https://auth.url", "test", "Sup3rS3cureTest!")).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _companyRepository.GetCallbackEditData(_identity.CompanyId, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.CreateOnboardingServiceProviderDetails(_identity.CompanyId, "https://test.de", "https://auth.url", "test", A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetCallbackAddress_WithOspData_UpdatesEntry()
    {
        //Arrange
        OnboardingServiceProviderDetail? osp = null;
        A.CallTo(() => _companyRepository.GetCallbackEditData(A<Guid>._, A<CompanyRoleId>._))
            .Returns((true, new OspDetails("https://test-old.de", "https://auth.url", "test", "test123")));
        A.CallTo(() => _companyRepository.AttachAndModifyOnboardingServiceProvider(A<Guid>._, A<Action<OnboardingServiceProviderDetail>>._, A<Action<OnboardingServiceProviderDetail>>._))
            .Invokes((Guid companyId, Action<OnboardingServiceProviderDetail>? initialize, Action<OnboardingServiceProviderDetail> setOptionalFields) =>
            {
                osp = new OnboardingServiceProviderDetail(companyId, null!, null!, null!, null!);
                initialize?.Invoke(osp);
                setOptionalFields.Invoke(osp);
            });

        //Act
        await _logic.SetCallbackAddress(new OnboardingServiceProviderCallbackRequestData("https://test-new.de", "https//auth.url", "test", "Sup3rS3cureTest!")).ConfigureAwait(false);

        //Assert
        osp.Should().NotBeNull().And.Match<OnboardingServiceProviderDetail>(x =>
            x.CompanyId == _identity.CompanyId &&
            x.CallbackUrl == "https://test-new.de");
        A.CallTo(() => _companyRepository.GetCallbackEditData(_identity.CompanyId, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
    }

    #endregion
}
