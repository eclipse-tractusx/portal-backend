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
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BpdmServiceTests
{
    #region Initialization

    private readonly ITokenService _tokenService;
    private readonly IOptions<BpdmServiceSettings> _options;
    private readonly IFixture _fixture;

    public BpdmServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new BpdmServiceSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            TokenAddress = "https://key.cloak.com",
        });
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region Trigger PutInputLegalEntity

    [Fact]
    public async Task PutInputLegalEntity_WithValidData_DoesNotThrowException()
    {
        // Arrange
        var data = _fixture.Create<BpdmTransferData>();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var result = await sut.PutInputLegalEntity(data, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PutInputLegalEntity_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var data = _fixture.Create<BpdmTransferData>();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.PutInputLegalEntity(data, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Contain("call to external system bpdm-put-legal-entities failed with statuscode");
    }

    #endregion

    #region Trigger SetSharingStateToReady

    [Fact]
    public async Task SetSharingStateToReady_WithValidData_DoesNotThrowException()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var result = await sut.SetSharingStateToReady(externalId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetSharingStateToReady_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.SetSharingStateToReady(externalId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system bpdm-put-sharing-state-ready failed with statuscode 400");
    }

    #endregion

    #region FetchInputLegalEntity

    [Theory]
    [InlineData("DE234567890", "EU_VAT_ID_DE")]
    [InlineData("CZ12345678", "EU_VAT_ID_CZ")]
    [InlineData("PL1234567890", "EU_VAT_ID_PL")]
    [InlineData("BE1234567890", "EU_VAT_ID_BE")]
    [InlineData("CHE123456789", "EU_VAT_ID_CH")]
    [InlineData("DK12345678", "EU_VAT_ID_DK")]
    [InlineData("ES12345678", "EU_VAT_ID_ES")]
    [InlineData("GB12345678", "EU_VAT_ID_GB")]
    [InlineData("NO12345678", "EU_VAT_ID_NO")]
    [InlineData("FR12345678901", "EU_VAT_ID_FR")]
    [InlineData("CHE-123.456.789", "CH_UID")]
    [InlineData("12345678901", "FR_SIREN")]
    [InlineData("ATU12345678", "EU_VAT_ID_AT")]
    [InlineData("12345678", "DE_BNUM")]
    [InlineData("12345678", "CZ_ICO")]
    [InlineData("1234567890", "BE_ENT_NO")]
    [InlineData("12345678", "CVR_DK")]
    [InlineData("12345678", "ID_CRN")]
    [InlineData("12345678", "NO_ORGID")]
    [InlineData("12345678", "LEI_ID")]
    [InlineData("283329464", "DUNS_ID")]
    public async Task FetchInputLegalEntity_WithValidResult_ReturnsExpected(string identifierValue, string identifierType)
    {
        // Arrange
        const string externalId = "0bf60442-09a8-4f09-811b-8854626ed5a6";
        var json = @"{
            ""totalElements"": 1,
            ""totalPages"": 1,
            ""page"": 0,
            ""contentSize"": 1,
            ""content"": [
                {
                    ""legalNameParts"": [
                        ""Volkswagen AG""
                    ],
                    ""identifiers"": [
                        {
                            ""value"": ""replaceTestValue"",
                            ""type"": ""replaceTestType"",
                            ""issuingBody"": null
                        }
                    ],
                    ""legalShortName"": ""Volkswagen AG"",
                    ""legalForm"": null,
                    ""states"": [],
                    ""classifications"": [],
                    ""roles"": [],
                    ""legalAddress"": {
                        ""nameParts"": [],
                        ""states"": [],
                        ""identifiers"": [],
                        ""physicalPostalAddress"": {
                            ""geographicCoordinates"": null,
                            ""country"": ""DE"",
                            ""administrativeAreaLevel1"": null,
                            ""administrativeAreaLevel2"": null,
                            ""administrativeAreaLevel3"": null,
                            ""postalCode"": ""38440"",
                            ""city"": ""Wolfsburg"",
                            ""district"": null,
                            ""street"": {
                                ""namePrefix"": null,
                                ""additionalNamePrefix"": null,
                                ""name"": ""Berliner Ring 2"",
                                ""nameSuffix"": null,
                                ""additionalNameSuffix"": null,
                                ""houseNumber"": null,
                                ""milestone"": null,
                                ""direction"": null
                            },
                            ""companyPostalCode"": null,
                            ""industrialZone"": null,
                            ""building"": null,
                            ""floor"": null,
                            ""door"": null
                        },
                        ""alternativePostalAddress"": null,
                        ""roles"": [],
                        ""externalId"": ""0bf60442-09a8-4f09-811b-8854626ed5a6_legalAddress"",
                        ""legalEntityExternalId"": ""0bf60442-09a8-4f09-811b-8854626ed5a6"",
                        ""siteExternalId"": null,
                        ""bpna"": ""BPNA000000006GFG""
                    },
                    ""externalId"": ""0bf60442-09a8-4f09-811b-8854626ed5a6"",
                    ""bpnl"": ""BPNL00000007QGTF""
                }
            ]
        }";

        json = json.Replace("replaceTestValue", identifierValue).Replace("replaceTestType", identifierType);

        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent(json));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var result = await sut.FetchInputLegalEntity(externalId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ExternalId.Should().Be(externalId);
        result.LegalEntity?.Bpnl.Should().Be("BPNL00000007QGTF");
    }

    [Fact]
    public async Task FetchInputLegalEntity_WithEmtpyObjectResult_ThrowsServiceException()
    {
        // Arrange
        using var stringContent = new StringContent("{}");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            stringContent);

        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com"),
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.FetchInputLegalEntity(_fixture.Create<string>(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("Access to external system bpdm did not return a valid legal entity response");
        ex.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public async Task FetchInputLegalEntity_WithEmtpyResult_ThrowsServiceException()
    {
        // Arrange
        using var stringContent = new StringContent("");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            stringContent);

        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com"),
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.FetchInputLegalEntity(_fixture.Create<string>(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
        ex.IsRecoverable.Should().BeFalse();
    }

    [Fact]
    public async Task FetchInputLegalEntity_WithNotFoundResult_ReturnsNull()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.NotFound);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var Act = () => sut.FetchInputLegalEntity(_fixture.Create<string>(), CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<ServiceException>(Act);
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FetchInputLegalEntity_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.FetchInputLegalEntity(_fixture.Create<string>(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system bpdm-search-legal-entities failed with statuscode 400");
    }

    #endregion

    #region GetSharingState

    [Fact]
    public async Task GetSharingState_WithValidResult_ReturnsExpected()
    {
        // Arrange
        var applicationId = new Guid("aa2eeac5-a0e9-46b0-80f0-d48dde49aa23");
        const string json = @"{
            ""totalElements"": 1,
            ""totalPages"": 1,
            ""page"": 0,
            ""contentSize"": 1,
            ""content"": [
                {
                    ""businessPartnerType"": ""LEGAL_ENTITY"",
                    ""externalId"": ""aa2eeac5-a0e9-46b0-80f0-d48dde49aa23"",
                    ""sharingStateType"": ""Error"",
                    ""sharingErrorCode"": ""SharingProcessError"",
                    ""sharingErrorMessage"": ""Address Identifier Type 'Cheese Region' does not exist (LegalAddressRegionNotFound)"",
                    ""bpn"": null,
                    ""sharingProcessStarted"": ""2023-08-04T14:35:30.478594""
                }
            ]
        }";

        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent(json));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var result = await sut.GetSharingState(applicationId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ExternalId.Should().Be(applicationId);
        result.Bpn.Should().BeNull();
        result.SharingErrorCode.Should().Be("SharingProcessError");
        result.SharingStateType.Should().Be(BpdmSharingStateType.Error);
        result.BusinessPartnerType.Should().Be(BpdmSharingStateBusinessPartnerType.LEGAL_ENTITY);
        result.SharingErrorMessage.Should().Be("Address Identifier Type 'Cheese Region' does not exist (LegalAddressRegionNotFound)");
    }

    [Fact]
    public async Task GetSharingState_WithEmtpyObjectResult_ThrowsServiceException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        using var stringContent = new StringContent("{}");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            stringContent);

        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com"),
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.GetSharingState(applicationId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("Access to sharing state did not return a valid legal entity response");
        ex.IsRecoverable.Should().BeTrue();
    }

    [Fact]
    public async Task GetSharingState_WithEmtpyResult_ThrowsServiceException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        using var stringContent = new StringContent("");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            stringContent);

        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com"),
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.GetSharingState(applicationId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().StartWith("Access to sharing state did not return a valid json response");
        ex.IsRecoverable.Should().BeFalse();
    }

    [Fact]
    public async Task GetSharingState_WithNotFoundResult_ReturnsNull()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.NotFound);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var Act = () => sut.GetSharingState(applicationId, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<ServiceException>(Act);
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSharingState_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        async Task Act() => await sut.GetSharingState(applicationId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system bpdm-sharing-state failed with statuscode 400");
    }

    #endregion
}
