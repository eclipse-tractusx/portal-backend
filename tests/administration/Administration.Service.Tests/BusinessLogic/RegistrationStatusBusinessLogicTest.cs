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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
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

        _logic = new RegistrationStatusBusinessLogic(_portalRepositories, identityService);
    }

    #region GetCallbackAddress

    [Fact]
    public async Task GetCallbackAddress_WithExistingOspData_ReturnsExpectedCallbackUrl()
    {
        //Arrange
        A.CallTo(() => _companyRepository.GetCallbackData(_identity.CompanyId))
            .Returns(new OnboardingServiceProviderCallbackResponseData("https://callback-url.com"));

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
            .Returns(new OnboardingServiceProviderCallbackResponseData(null));

        //Act
        var result = await _logic.GetCallbackAddress().ConfigureAwait(false);

        //Assert
        result.CallbackUrl.Should().Be(null);
    }

    #endregion
    
    #region SetCallbackAddress

    [Fact]
    public async Task SetCallbackAddress_WithCompanyNotOsp_ReturnsExpectedCallbackUrl()
    {
        //Arrange
        A.CallTo(() => _companyRepository.GetCallbackEditData(_identity.CompanyId))
            .Returns(new ValueTuple<bool, bool, string?>());

        //Act
        async Task Act() => await _logic.SetCallbackAddress(new OnboardingServiceProviderCallbackData("https://test.de")).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Only {CompanyRoleId.ONBOARDING_SERVICE_PROVIDER} are allowed to set the callback url");
    }

    [Fact]
    public async Task SetCallbackAddress_WithNonExistingOspData_InsertExpected()
    {
        //Arrange
        A.CallTo(() => _companyRepository.GetCallbackEditData(_identity.CompanyId))
            .Returns(new ValueTuple<bool, bool, string?>(true, false, null));

        //Act
        await _logic.SetCallbackAddress(new OnboardingServiceProviderCallbackData("https://test.de")).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _companyRepository.CreateOnboardingServiceProviderDetails(_identity.CompanyId, "https://test.de"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("https://test-old.de")]
    [InlineData(null)]
    public async Task SetCallbackAddress_WithOspData_UpdatesEntry(string? existingUrl)
    {
        //Arrange
        var osp = new OnboardingServiceProviderDetail(_identity.CompanyId, null!);
        A.CallTo(() => _companyRepository.GetCallbackEditData(_identity.CompanyId))
            .Returns(new ValueTuple<bool, bool, string?>(true, true, existingUrl));
        A.CallTo(() => _companyRepository.AttachAndModifyOnboardingServiceProvider(_identity.CompanyId, A<Action<OnboardingServiceProviderDetail>>._, A<Action<OnboardingServiceProviderDetail>>._))
            .Invokes((Guid _, Action<OnboardingServiceProviderDetail>? initialize, Action<OnboardingServiceProviderDetail> setOptionalFields) =>
            {
                initialize?.Invoke(osp);
                setOptionalFields.Invoke(osp);
            });
        
        //Act
        await _logic.SetCallbackAddress(new OnboardingServiceProviderCallbackData("https://test-new.de")).ConfigureAwait(false);

        //Assert
        osp.CallbackUrl.Should().Be("https://test-new.de");
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
    }

    #endregion
}
