/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;

public class BpdmService : IBpdmService
{
    private readonly ITokenService _tokenService;
    private readonly BpdmServiceSettings _settings;
    private readonly JsonSerializerOptions _options = new (){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public BpdmService(ITokenService tokenService, IOptions<BpdmServiceSettings> options)
    {
        _tokenService = tokenService;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> PutInputLegalEntity(BpdmTransferData data, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<BpdmService>(_settings, cancellationToken).ConfigureAwait(false);

        try
        {
            var requestData = new BpdmLegalEntityData[]
            {
                new(
                    data.ExternalId,               // ExternalId
                    null,                          // Bpn
                    data.Identifiers.Select(x =>
                        new BpdmIdentifier(
                            x.Value,               // Value
                            x.BpdmIdentifierId,    // Type
                            null,                  // IssuingBody
                            null)),                // Status
                    new BpdmName[] // Names
                    {
                        new (
                            data.CompanyName,      // Value
                            null,                  // ShortName
                            "REGISTERED",          // Type
                            "de")                  // Language
                    },
                    null, // LegalForm
                    null, // Status
                    Enumerable.Empty<BpdmProfileClassification>(),
                    Enumerable.Empty<string>(),    // Types
                    Enumerable.Empty<BpdmBankAccount>(),
                    new BpdmLegalAddress(
                        new BpdmAddressVersion(
                            "WESTERN_LATIN_STANDARD", //CharacterSet
                            "de"),                    // Version
                        null,                      // CareOf
                        Enumerable.Empty<string>(),// Contexts
                        data.AlphaCode2,           // Country
                        data.Region == null
                            ? Enumerable.Empty<BpdmAdministrativeArea>()
                            : new BpdmAdministrativeArea[] {
                                new (
                                    data.Region,   // Value
                                    null,          // ShortName
                                    null,          // Fipscode
                                    "COUNTY"       // Type
                                )
                            },
                        data.ZipCode == null
                            ? Enumerable.Empty<BpdmPostcode>()
                            : new BpdmPostcode[] {
                                new (
                                    data.ZipCode,  // Value
                                    "REGULAR")     // Type
                            },
                        new BpdmLocality[] {
                            new (
                                data.City,         // Value
                                null,              // ShortName
                                "CITY")            // Type
                        },
                        new BpdmThoroughfare[] {
                            new (
                                data.StreetName,   // Value
                                null,              // Name
                                null,              // ShortName
                                data.StreetNumber, // Number
                                null,              // Direction
                                "STREET")          // Type
                        },
                        Enumerable.Empty<BpdmPremise>(),
                        Enumerable.Empty<BpdmPostalDeliveryPoint>(),
                        null,                 // GeographicCoordinates
                        Enumerable.Empty<string>() // Types
                        )
                )
            };
        
            var result = await httpClient.PutAsJsonAsync("api/catena/input/legal-entities", requestData, _options, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
                return true;

            throw new ServiceException("Bpdm Service Call failed with StatusCode", result.StatusCode);
        }
        catch (Exception ex)
        {
            if (ex is ServiceException)
            {
                throw;
            }
            throw new ServiceException("Bpdm Service Call failed.", ex);
        }
    }

    public async Task<BpdmLegalEntityData?> FetchInputLegalEntity(string externalId, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<BpdmService>(_settings, cancellationToken).ConfigureAwait(false);
        var result = await httpClient.GetAsync($"api/catena/input/legal-entities/{externalId}", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccessStatusCode)
        {
            if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            throw new ServiceException($"Access to external system bpdm failed with Status Code {result.StatusCode}", result.StatusCode);
        }
        await using var responseStream = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var legalEntityResponse = await JsonSerializer.DeserializeAsync<BpdmLegalEntityData>(
                responseStream,
                _options,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (legalEntityResponse?.ExternalId == null)
            {
                throw new ServiceException("Access to external system bpdm did not return a valid legal entity response");
            }
            return legalEntityResponse;
        }
        catch(JsonException je)
        {
            throw new ServiceException($"Access to external system bpdm did not return a valid json response: {je.Message}");
        }
    }
}
