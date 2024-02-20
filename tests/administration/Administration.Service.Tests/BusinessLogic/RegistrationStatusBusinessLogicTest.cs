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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class RegistrationStatusBusinessLogicTest
{
    private readonly IIdentityData _identity;

    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyRepository _companyRepository;
    private readonly OnboardingServiceProviderSettings _options;
    private readonly IRegistrationStatusBusinessLogic _logic;

    private readonly IFixture _fixture;
    public RegistrationStatusBusinessLogicTest()
    {
        _fixture = new Fixture();
        _identity = A.Fake<IIdentityData>();
        var identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        A.CallTo(() => identityService.IdentityData).Returns(_identity);

        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);

        _options = new OnboardingServiceProviderSettings
        {
            EncryptionConfigs = new EncryptionModeConfig[]
            {
                new() { Index=0, EncryptionKey=Convert.ToHexString(_fixture.CreateMany<byte>(32).ToArray()), CipherMode=CipherMode.ECB, PaddingMode=PaddingMode.PKCS7 },
                new() { Index=1, EncryptionKey=Convert.ToHexString(_fixture.CreateMany<byte>(32).ToArray()), CipherMode=CipherMode.CBC, PaddingMode=PaddingMode.PKCS7 },
            },
            EncrptionConfigIndex = 1
        };
        _logic = new RegistrationStatusBusinessLogic(_portalRepositories, identityService, Options.Create(_options));
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
            .Returns<(bool, Guid?, OspDetails?)>(default);

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
            .Returns((true, null, null));

        //Act
        await _logic.SetCallbackAddress(new OnboardingServiceProviderCallbackRequestData("https://test.de", "https://auth.url", "test", "Sup3rS3cureTest!")).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _companyRepository.GetCallbackEditData(_identity.CompanyId, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companyRepository.CreateOnboardingServiceProviderDetails(_identity.CompanyId, "https://test.de", "https://auth.url", "test", A<byte[]>._, A<byte[]>._, A<int>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetCallbackAddress_WithOspData_UpdatesEntry()
    {
        //Arrange
        var onboardingServiceProviderDetailId = Guid.NewGuid();
        OnboardingServiceProviderDetail? initial = null;
        OnboardingServiceProviderDetail? updated = null;
        var oldsecret = _fixture.CreateMany<byte>(32).ToArray();
        var clientSecret = _fixture.Create<string>();
        A.CallTo(() => _companyRepository.GetCallbackEditData(A<Guid>._, A<CompanyRoleId>._))
            .Returns((true, onboardingServiceProviderDetailId, new OspDetails("https://test-old.de", "https://auth.url", "test", oldsecret, null, 0)));
        A.CallTo(() => _companyRepository.AttachAndModifyOnboardingServiceProvider(A<Guid>._, A<Action<OnboardingServiceProviderDetail>>._, A<Action<OnboardingServiceProviderDetail>>._))
            .Invokes((Guid onboardingServiceProviderDetailId, Action<OnboardingServiceProviderDetail>? initialize, Action<OnboardingServiceProviderDetail> setOptionalFields) =>
            {
                initial = new OnboardingServiceProviderDetail(onboardingServiceProviderDetailId, Guid.Empty, null!, null!, null!, null!, null, default);
                updated = new OnboardingServiceProviderDetail(onboardingServiceProviderDetailId, Guid.Empty, null!, null!, null!, null!, null, default);
                initialize?.Invoke(initial);
                setOptionalFields.Invoke(updated);
            });

        //Act
        await _logic.SetCallbackAddress(new OnboardingServiceProviderCallbackRequestData("https://test-new.de", "https//auth.url", "test", clientSecret)).ConfigureAwait(false);

        updated.Should().NotBeNull();

        //Assert
        initial.Should().NotBeNull().And.Match<OnboardingServiceProviderDetail>(x =>
            x.Id == onboardingServiceProviderDetailId &&
            x.CallbackUrl == "https://test-old.de" &&
            x.ClientSecret.SequenceEqual(oldsecret) &&
            x.EncryptionMode == 0);
        updated.Should().NotBeNull().And.Match<OnboardingServiceProviderDetail>(x =>
            x.Id == onboardingServiceProviderDetailId &&
            x.CallbackUrl == "https://test-new.de" &&
            x.EncryptionMode == 1);
        A.CallTo(() => _companyRepository.GetCallbackEditData(_identity.CompanyId, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync())
            .MustHaveHappenedOnceExactly();

        var cryptoConfig = _options.EncryptionConfigs.ElementAtOrDefault(_options.EncrptionConfigIndex);
        cryptoConfig.Should().NotBeNull()
            .And.Match<EncryptionModeConfig>(x =>
                x.Index == 1 &&
                x.CipherMode == CipherMode.CBC &&
                x.PaddingMode == PaddingMode.PKCS7
            );

        var result = CryptoHelper.Decrypt(updated!.ClientSecret, updated.InitializationVector, Convert.FromHexString(cryptoConfig!.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        result.Should().Be(clientSecret);
    }

    #endregion
}
