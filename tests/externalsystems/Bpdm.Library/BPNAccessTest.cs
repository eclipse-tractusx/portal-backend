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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Tests;

public class BPNAccessTest
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });

    #region FetchLegalEntityByBpns

    [Fact]
    public async Task FetchLegalEntityByBpn_Success_ReturnsExpected()
    {
        //Arrange
        HttpRequestMessage? request = null;

        const string json = @"{
            ""bpnl"": ""BPNL000000000001"",
            ""legalName"": ""Comapany Test Auto"",
            ""legalShortName"": ""CTA"",
            ""legalForm"": {
                ""technicalKey"": ""CUSTOM_LEGAL_FORM_f254bb28-92f2-4b49-81a8-f364cde5a5cc"",
                ""name"": ""Legal Form for Test Automation"",
                ""abbreviation"": null
            },
            ""identifiers"": [
                {
                    ""value"": ""1c4815e7-c5b6-41a3-862a-a815ee2a836e"",
                    ""type"": {
                        ""technicalKey"": ""CUSTOM_LE_ID_e6e534ac-ff59-40ab-bd0d-da94f73f700e"",
                        ""name"": ""Custom Identifier Type of LE for Test Automation""
                    },
                    ""issuingBody"": ""ISSUE_BODY_TEST_AUTO""
                }
            ],
            ""states"": [
                {
                    ""validFrom"": ""2023-07-16T05:54:48.942"",
                    ""validTo"": ""2024-06-09T07:31:01.213"",
                    ""type"": {
                        ""technicalKey"": ""ACTIVE"",
                        ""name"": ""Active""
                    }
                }
            ],
            ""relations"": [],
            ""currentness"": ""2023-09-20T05:31:17.009357Z"",
            ""confidenceCriteria"": {
                ""sharedByOwner"": false,
                ""checkedByExternalDataSource"": false,
                ""numberOfSharingMembers"": 1,
                ""lastConfidenceCheckAt"": ""2023-12-29T07:56:51.44798"",
                ""nextConfidenceCheckAt"": ""2023-12-29T07:56:51.44798"",
                ""confidenceLevel"": 0
            },
            ""isCatenaXMemberData"": true,
            ""createdAt"": ""2023-09-20T05:31:17.090516Z"",
            ""updatedAt"": ""2023-09-20T05:31:17.090523Z"",
            ""legalAddress"": {
                ""bpna"": ""BPNA000000000001"",
                ""name"": ""ADDRESS_TEST_AUTO"",
                ""states"": [
                    {
                        ""validFrom"": ""2023-07-16T05:54:48.942"",
                        ""validTo"": ""2024-06-05T07:31:01.213"",
                        ""type"": {
                            ""technicalKey"": ""ACTIVE"",
                            ""name"": ""Active""
                        }
                    }
                ],
                ""identifiers"": [
                    {
                        ""value"": ""1c4815e7-c5b6-41a3-862a-a815ee2a836e"",
                        ""type"": {
                            ""technicalKey"": ""CUSTOM_ADD_ID_0501e165-a446-40f9-b49b-63158abec717"",
                            ""name"": ""Custom Identifier Type of Test Automation""
                        }
                    }
                ],
                ""physicalPostalAddress"": {
                    ""geographicCoordinates"": {
                        ""longitude"": 0.0,
                        ""latitude"": 0.0,
                        ""altitude"": 0.0
                    },
                    ""country"": {
                        ""technicalKey"": ""DE"",
                        ""name"": ""Germany""
                    },
                    ""administrativeAreaLevel1"": null,
                    ""administrativeAreaLevel2"": ""test1"",
                    ""administrativeAreaLevel3"": ""test2"",
                    ""postalCode"": ""1111"",
                    ""city"": ""TestCity"",
                    ""district"": ""Test district"",
                    ""street"": {
                        ""name"": ""Stuttgarter Strasse"",
                        ""houseNumber"": ""1"",
                        ""houseNumberSupplement"": null,
                        ""milestone"": ""Test milestone 1"",
                        ""direction"": ""Test direction 1"",
                        ""namePrefix"": null,
                        ""additionalNamePrefix"": null,
                        ""nameSuffix"": null,
                        ""additionalNameSuffix"": null
                    },
                    ""companyPostalCode"": ""1234"",
                    ""industrialZone"": ""Test industrialZone 1"",
                    ""building"": ""Test building 1"",
                    ""floor"": ""F"",
                    ""door"": ""test door 1""
                },
                ""alternativePostalAddress"": {
                    ""geographicCoordinates"": {
                        ""longitude"": 0.0,
                        ""latitude"": 0.0,
                        ""altitude"": 0.0
                    },
                    ""country"": {
                        ""technicalKey"": ""DE"",
                        ""name"": ""Germany""
                    },
                    ""administrativeAreaLevel1"": null,
                    ""postalCode"": ""2222"",
                    ""city"": ""Test city 2"",
                    ""deliveryServiceType"": ""PO_BOX"",
                    ""deliveryServiceQualifier"": ""test deliveryServiceQualifier"",
                    ""deliveryServiceNumber"": ""2222""
                },
                ""bpnLegalEntity"": ""BPNL000000000001"",
                ""bpnSite"": null,
                ""isCatenaXMemberData"": true,
                ""createdAt"": ""2023-09-20T05:31:17.084880Z"",
                ""updatedAt"": ""2023-09-20T05:31:17.096188Z"",
                ""confidenceCriteria"": {
                    ""sharedByOwner"": false,
                    ""checkedByExternalDataSource"": false,
                    ""numberOfSharingMembers"": 1,
                    ""lastConfidenceCheckAt"": ""2023-12-29T07:56:51.44798"",
                    ""nextConfidenceCheckAt"": ""2023-12-29T07:56:51.44798"",
                    ""confidenceLevel"": 0
                },
                ""addressType"": ""LegalAddress""
            }
        }";

        using var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        };
        _fixture.ConfigureHttpClientFactoryFixture("bpn", responseMessage, requestMessage => request = requestMessage);

        var businessPartnerNumber = _fixture.Create<string>();
        var sut = _fixture.Create<BpnAccess>();

        //Act
        var result = await sut.FetchLegalEntityByBpn(businessPartnerNumber, _fixture.Create<string>(), CancellationToken.None);

        //Assert
        request.Should().NotBeNull();
        request!.RequestUri.Should().NotBeNull();
        request.RequestUri.Should().NotBeNull();
        request.RequestUri!.LocalPath.Should().Be($"/legal-entities/{businessPartnerNumber}");
        request.RequestUri.Query.Should().Be("?idType=BPN");

        result.Should().NotBeNull();
        result.LegalEntityAddress.Should().NotBeNull();
        result.LegalEntityAddress!.Bpna.Should().Be("BPNA000000000001");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_InvalidJsonResponse_Throws()
    {
        //Arrange
        var json = _fixture.Create<string>();
        using var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        };
        _fixture.ConfigureHttpClientFactoryFixture("bpn", responseMessage);

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_EmptyResponse_Throws()
    {
        //Arrange
        const string json = "";
        using var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        };
        _fixture.ConfigureHttpClientFactoryFixture("bpn", responseMessage);

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_NoContentResponse_Throws()
    {
        //Arrange
        using var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
        };
        _fixture.ConfigureHttpClientFactoryFixture("bpn", responseMessage);

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid json response");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_InvalidJsonType_Throws()
    {
        //Arrange
        const string json = "{\"some\": [{\"other\": \"json\"}]}";
        using var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        };
        _fixture.ConfigureHttpClientFactoryFixture("bpn", responseMessage);

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act);

        //Assert
        result.Should().NotBeNull();
        result.Message.Should().StartWith("Access to external system bpdm did not return a valid legal entity response");
    }

    [Fact]
    public async Task FetchLegalEntityByBpn_UnsuccessfulCall_Throws()
    {
        //Arrange
        var json = _fixture.Create<string>();
        using var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(json)
        };
        _fixture.ConfigureHttpClientFactoryFixture("bpn", responseMessage);

        var sut = _fixture.Create<BpnAccess>();

        var Act = () => sut.FetchLegalEntityByBpn(_fixture.Create<string>(), _fixture.Create<string>(), CancellationToken.None);

        //Act
        var result = await Assert.ThrowsAsync<ServiceException>(Act);

        //Assert
        result.Message.Should().Be($"call to external system bpn-fetch-legal-entity failed with statuscode {(int)HttpStatusCode.BadRequest}");
    }

    #endregion
}
