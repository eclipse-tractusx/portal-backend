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
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
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
        _fixture.Inject(_options);
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
        HttpRequestMessage? request = null;
        _fixture.ConfigureTokenServiceFixture<BpdmService>(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        }, requestMessage => request = requestMessage);
        var sut = _fixture.Create<BpdmService>();

        // Act
        var result = await sut.SetSharingStateToReady(externalId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        request.Should().NotBeNull();
        request!.RequestUri.Should()
            .Be("https://example.com/path/test/sharing-state/ready");
    }

    [Fact]
    public async Task SetSharingStateToReady_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var externalId = Guid.NewGuid().ToString();
        HttpRequestMessage? request = null;
        _fixture.ConfigureTokenServiceFixture<BpdmService>(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest
        }, requestMessage => request = requestMessage);
        var sut = _fixture.Create<BpdmService>();

        // Act
        async Task Act() => await sut.SetSharingStateToReady(externalId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("call to external system bpdm-put-sharing-state-ready failed with statuscode 400");
        request!.RequestUri.Should()
            .Be("https://example.com/path/test/sharing-state/ready");
    }

    #endregion

    #region FetchInputLegalEntity

    [Fact]
    public async Task FetchInputLegalEntity_WithValidResult_ReturnsExpected()
    {
        // Arrange
        const string ExternalId = "0bf60442-09a8-4f09-811b-8854626ed5a6";
        const string Json = @"{
            ""totalElements"": 1,
            ""totalPages"": 1,
            ""page"": 0,
            ""contentSize"": 1,
            ""content"": [
              {
                ""externalId"": ""0bf60442-09a8-4f09-811b-8854626ed5a6"",
                ""nameParts"": [
                    ""Volkswagen AG""
                ],
                ""identifiers"": [
                  {
                    ""type"": ""EU_VAT_ID_DE"",
                    ""value"": ""DE234567890"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_CZ"",
                    ""value"": ""CZ12345678"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_PL"",
                    ""value"": ""PL1234567890"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_BE"",
                    ""value"": ""BE1234567890"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_CH"",
                    ""value"": ""CHE123456789"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_DK"",
                    ""value"": ""DK12345678"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_ES"",
                    ""value"": ""ES12345678"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_GB"",
                    ""value"": ""GB12345678"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_NO"",
                    ""value"": ""NO12345678"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_FR"",
                    ""value"": ""FR12345678901"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""CH_UID"",
                    ""value"": ""CHE-123.456.789"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""FR_SIREN"",
                    ""value"": ""12345678901"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""EU_VAT_ID_AT"",
                    ""value"": ""ATU12345678"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""DE_BNUM"",
                    ""value"": ""12345670"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""CZ_ICO"",
                    ""value"": ""12345671"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""BE_ENT_NO"",
                    ""value"": ""1234567890"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""CVR_DK"",
                    ""value"": ""12345672"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""ID_CRN"",
                    ""value"": ""12345673"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""NO_ORGID"",
                    ""value"": ""12345674"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""LEI_ID"",
                    ""value"": ""12345675"",
                    ""issuingBody"": ""string""
                  },
                  {
                    ""type"": ""DUNS_ID"",
                    ""value"": ""283329464"",
                    ""issuingBody"": ""string""
                  }
                ],
                ""states"": [
                  {
                    ""validFrom"": ""2024-04-12T09:48:11.443Z"",
                    ""validTo"": ""2024-04-12T09:48:11.443Z"",
                    ""type"": ""ACTIVE""
                  }
                ],
                ""roles"": [
                    ""SUPPLIER""
                ],
                ""isOwnCompanyData"": true,
                ""legalEntity"": {
                    ""legalEntityBpn"": ""BPNL00000007QGTF"",
                    ""legalName"": ""Volkswagen AG"",
                    ""shortName"": ""VW"",
                    ""legalForm"": ""string"",
                    ""classifications"": [
                      {
                        ""type"": ""NACE"",
                        ""code"": ""string"",
                        ""value"": ""string""
                      }
                    ],
                    ""confidenceCriteria"": {
                        ""sharedByOwner"": true,
                        ""checkedByExternalDataSource"": true,
                        ""numberOfBusinessPartners"": 0,
                        ""lastConfidenceCheckAt"": ""2024-04-12T09:48:11.443Z"",
                        ""nextConfidenceCheckAt"": ""2024-04-12T09:48:11.443Z"",
                        ""confidenceLevel"": 0
                    }
                },
                ""site"": {
                    ""siteBpn"": ""string"",
                    ""name"": ""string"",
                    ""confidenceCriteria"": {
                        ""sharedByOwner"": true,
                        ""checkedByExternalDataSource"": true,
                        ""numberOfBusinessPartners"": 0,
                        ""lastConfidenceCheckAt"": ""2024-04-12T09:48:11.443Z"",
                        ""nextConfidenceCheckAt"": ""2024-04-12T09:48:11.443Z"",
                        ""confidenceLevel"": 0
                  }
                },
                ""address"": {
                    ""addressBpn"": ""BPNA000000006GFG"",
                    ""name"": ""string"",
                    ""addressType"": ""LegalAndSiteMainAddress"",
                    ""physicalPostalAddress"": {
                        ""geographicCoordinates"": {
                            ""longitude"": 0,
                            ""latitude"": 0,
                            ""altitude"": 0
                        },
                    ""country"": ""UNDEFINED"",
                    ""administrativeAreaLevel1"": ""string"",
                    ""administrativeAreaLevel2"": ""string"",
                    ""administrativeAreaLevel3"": ""string"",
                    ""postalCode"": ""38440"",
                    ""city"": ""Wolfsburg"",
                    ""district"": ""string"",
                    ""street"": {
                        ""namePrefix"": ""string"",
                        ""additionalNamePrefix"": ""string"",
                        ""name"": ""Berliner Ring"",
                        ""nameSuffix"": ""string"",
                        ""additionalNameSuffix"": ""string"",
                        ""houseNumber"": ""2"",
                        ""houseNumberSupplement"": ""string"",
                        ""milestone"": ""string"",
                        ""direction"": ""string""
                    },
                    ""companyPostalCode"": ""string"",
                    ""industrialZone"": ""string"",
                    ""building"": ""string"",
                    ""floor"": ""string"",
                    ""door"": ""string""
                    },
                    ""alternativePostalAddress"": {
                        ""geographicCoordinates"": {
                            ""longitude"": 0,
                            ""latitude"": 0,
                            ""altitude"": 0
                        },
                        ""country"": ""UNDEFINED"",
                        ""administrativeAreaLevel1"": ""string"",
                        ""postalCode"": ""string"",
                        ""city"": ""string"",
                        ""deliveryServiceType"": ""PO_BOX"",
                        ""deliveryServiceQualifier"": ""string"",
                        ""deliveryServiceNumber"": ""string""
                    },
                    ""confidenceCriteria"": {
                        ""sharedByOwner"": true,
                        ""checkedByExternalDataSource"": true,
                        ""numberOfBusinessPartners"": 0,
                        ""lastConfidenceCheckAt"": ""2024-04-12T09:48:11.443Z"",
                        ""nextConfidenceCheckAt"": ""2024-04-12T09:48:11.443Z"",
                        ""confidenceLevel"": 0
                    }
                },
                ""createdAt"": ""2024-04-12T09:48:11.443Z"",
                ""updatedAt"": ""2024-04-12T09:48:11.443Z""
              }
            ]
          }";

        var httpMessageHandlerMock = new HttpMessageHandlerMock(
            HttpStatusCode.OK,
            new StringContent(Json));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _tokenService.GetAuthorizedClient<BpdmService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new BpdmService(_tokenService, _options);

        // Act
        var result = await sut.FetchInputLegalEntity(ExternalId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ExternalId.Should().Be(ExternalId);
        result.LegalEntity.Should().NotBeNull();
        result.LegalEntity!.Bpnl.Should().Be("BPNL00000007QGTF");
        result.Identifiers.Should().HaveCount(21)
            .And.Satisfy(
                x => x.Type == "EU_VAT_ID_DE" && x.Value == "DE234567890",
                x => x.Type == "EU_VAT_ID_CZ" && x.Value == "CZ12345678",
                x => x.Type == "EU_VAT_ID_PL" && x.Value == "PL1234567890",
                x => x.Type == "EU_VAT_ID_BE" && x.Value == "BE1234567890",
                x => x.Type == "EU_VAT_ID_CH" && x.Value == "CHE123456789",
                x => x.Type == "EU_VAT_ID_DK" && x.Value == "DK12345678",
                x => x.Type == "EU_VAT_ID_ES" && x.Value == "ES12345678",
                x => x.Type == "EU_VAT_ID_GB" && x.Value == "GB12345678",
                x => x.Type == "EU_VAT_ID_NO" && x.Value == "NO12345678",
                x => x.Type == "EU_VAT_ID_FR" && x.Value == "FR12345678901",
                x => x.Type == "CH_UID" && x.Value == "CHE-123.456.789",
                x => x.Type == "FR_SIREN" && x.Value == "12345678901",
                x => x.Type == "EU_VAT_ID_AT" && x.Value == "ATU12345678",
                x => x.Type == "DE_BNUM" && x.Value == "12345670",
                x => x.Type == "CZ_ICO" && x.Value == "12345671",
                x => x.Type == "BE_ENT_NO" && x.Value == "1234567890",
                x => x.Type == "CVR_DK" && x.Value == "12345672",
                x => x.Type == "ID_CRN" && x.Value == "12345673",
                x => x.Type == "NO_ORGID" && x.Value == "12345674",
                x => x.Type == "LEI_ID" && x.Value == "12345675",
                x => x.Type == "DUNS_ID" && x.Value == "283329464"
            );
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
                    ""externalId"": ""aa2eeac5-a0e9-46b0-80f0-d48dde49aa23"",
                    ""sharingStateType"": ""Error"",
                    ""sharingErrorCode"": ""SharingProcessError"",
                    ""sharingErrorMessage"": ""Address Identifier Type 'Cheese Region' does not exist (LegalAddressRegionNotFound)"",
                    ""sharingProcessStarted"": ""2023-08-04T14:35:30.478594"",
                    ""taskId"": ""aa2eeac5-a0e9-46b0-80f0-d48dde49aa11""
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
        result.SharingErrorCode.Should().Be("SharingProcessError");
        result.SharingStateType.Should().Be(BpdmSharingStateType.Error);
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
