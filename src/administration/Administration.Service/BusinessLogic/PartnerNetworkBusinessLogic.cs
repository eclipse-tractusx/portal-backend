/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class PartnerNetworkBusinessLogic : IPartnerNetworkBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpnAccess _bpnAccess;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalRepositories"></param>
    /// <param name="bpnAccess"></param>
    public PartnerNetworkBusinessLogic(IPortalRepositories portalRepositories, IBpnAccess bpnAccess)
    {
        _portalRepositories = portalRepositories;
        _bpnAccess = bpnAccess;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<string> GetAllMemberCompaniesBPNAsync(IEnumerable<string>? bpnIds) =>
        _portalRepositories.GetInstance<ICompanyRepository>().GetAllMemberCompaniesBPNAsync(bpnIds?.Select(x => x.ToUpper()));

    /// <inheritdoc/>
    public async Task<PartnerNetworkResponse> GetPartnerNetworkDataAsync(int page, int size, PartnerNetworkRequest partnerNetworkRequest, string token, CancellationToken cancellationToken)
    {
        var data = await _bpnAccess.FetchPartnerNetworkData(page, size, partnerNetworkRequest.Bpnls, partnerNetworkRequest.LegalName, token, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return new PartnerNetworkResponse(ParsePartnerNetworkData(data.Content), data.ContentSize, data.Page, data.TotalElements, data.TotalPages);
    }

    private static IEnumerable<PartnerNetworkData> ParsePartnerNetworkData(IEnumerable<BpdmLegalEntityDto> bpdmLegalEntityDtos) =>
        bpdmLegalEntityDtos.Select(data =>
            new PartnerNetworkData(
                data.Bpn,
                data.LegalName,
                data.LegalShortName,
                data.Currentness,
                data.CreatedAt,
                data.UpdatedAt,
                data.Identifiers.Select(identifier =>
                    new BpdmIdentifierData(
                        identifier.Value,
                        new BpdmTechnicalKeyData(identifier.Type.TechnicalKey, identifier.Type.Name),
                        identifier.IssuingBody
                    )),
                data.LegalForm != null ? new BpdmLegalFormData(
                    data.LegalForm.TechnicalKey,
                    data.LegalForm.Name,
                    data.LegalForm.Abbreviation
                ) : null,
                data.States.Select(state =>
                    new BpdmStatusData(
                        state.ValidFrom,
                        state.ValidTo,
                        new BpdmTechnicalKeyData(state!.Type!.TechnicalKey, state!.Type!.Name)
                    )),
                data.ConfidenceCriteria != null ? new BpdmConfidenceCriteriaData(
                    data.ConfidenceCriteria.SharedByOwner,
                    data.ConfidenceCriteria.CheckedByExternalDataSource,
                    data.ConfidenceCriteria.NumberOfSharingMembers,
                    data.ConfidenceCriteria.LastConfidenceCheckAt,
                    data.ConfidenceCriteria.NextConfidenceCheckAt,
                    data.ConfidenceCriteria.ConfidenceLevel
                ) : null,
                data.IsCatenaXMemberData,
                data.Relations.Select(relation =>
                    new BpdmRelationData(
                        new BpdmTechnicalKeyData(relation.Type.TechnicalKey, relation.Type.Name),
                        relation.StartBpnl,
                        relation.EndBpnl,
                        relation.ValidFrom,
                        relation.ValidTo
                    )),
                data.LegalEntityAddress != null ? new BpdmLegalEntityAddressData(
                    data.LegalEntityAddress.Bpna,
                    data.LegalEntityAddress.Name,
                    data.LegalEntityAddress.BpnLegalEntity,
                    data.LegalEntityAddress.BpnSite,
                    data.LegalEntityAddress.CreatedAt,
                    data.LegalEntityAddress.UpdatedAt,
                    data.LegalEntityAddress.AddressType,
                    data.LegalEntityAddress.States.Select(state =>
                        new BpdmLegalEntityAddressStateData(
                            state.Description,
                            state.ValidFrom,
                            state.ValidTo,
                            new BpdmTechnicalKeyData(state.Type.TechnicalKey, state.Type.Name)
                        )),
                    data.LegalEntityAddress.Identifiers.Select(identifier =>
                        new BpdmLegalEntityAddressIdentifierData(
                            identifier.Value,
                            new BpdmTechnicalKeyData(identifier.Type.TechnicalKey, identifier.Type.Name)
                        )),
                    data.LegalEntityAddress.PhysicalPostalAddress != null ? new BpdmPhysicalPostalAddressData(
                        data.LegalEntityAddress.PhysicalPostalAddress.GeographicCoordinates != null ? new BpdmGeographicCoordinatesData(
                            data.LegalEntityAddress.PhysicalPostalAddress.GeographicCoordinates.Longitude,
                            data.LegalEntityAddress.PhysicalPostalAddress.GeographicCoordinates.Latitude,
                            data.LegalEntityAddress.PhysicalPostalAddress.GeographicCoordinates.Altitude
                        ) : null,
                        data.LegalEntityAddress.PhysicalPostalAddress.Country != null ? new BpdmCountryData(
                            data.LegalEntityAddress.PhysicalPostalAddress.Country.TechnicalKey,
                            data.LegalEntityAddress.PhysicalPostalAddress.Country.Name
                        ) : null,
                        data.LegalEntityAddress.PhysicalPostalAddress.PostalCode,
                        data.LegalEntityAddress.PhysicalPostalAddress.City,
                        data.LegalEntityAddress.PhysicalPostalAddress.Street != null ? new BpdmStreetData(
                            data.LegalEntityAddress.PhysicalPostalAddress.Street.Name,
                            data.LegalEntityAddress.PhysicalPostalAddress.Street.HouseNumber,
                            data.LegalEntityAddress.PhysicalPostalAddress.Street.Milestone,
                            data.LegalEntityAddress.PhysicalPostalAddress.Street.Direction
                        ) : null,
                        data.LegalEntityAddress.PhysicalPostalAddress.AdministrativeAreaLevel1 != null ? new BpdmAdministrativeAreaLevelData(
                            data.LegalEntityAddress.PhysicalPostalAddress.AdministrativeAreaLevel1.CountryCode,
                            data.LegalEntityAddress.PhysicalPostalAddress.AdministrativeAreaLevel1.RegionName,
                            data.LegalEntityAddress.PhysicalPostalAddress.AdministrativeAreaLevel1.RegionCode
                        ) : null,
                        data.LegalEntityAddress.PhysicalPostalAddress.AdministrativeAreaLevel2,
                        data.LegalEntityAddress.PhysicalPostalAddress.AdministrativeAreaLevel3,
                        data.LegalEntityAddress.PhysicalPostalAddress.District,
                        data.LegalEntityAddress.PhysicalPostalAddress.CompanyPostalCode,
                        data.LegalEntityAddress.PhysicalPostalAddress.IndustrialZone,
                        data.LegalEntityAddress.PhysicalPostalAddress.Building,
                        data.LegalEntityAddress.PhysicalPostalAddress.Floor,
                        data.LegalEntityAddress.PhysicalPostalAddress.Door
                    ) : null,
                    data.LegalEntityAddress.AlternativePostalAddress != null ? new BpdmAlternativePostalAddressData(
                        data.LegalEntityAddress.AlternativePostalAddress.GeographicCoordinates != null ? new BpdmGeographicCoordinatesData(
                            data.LegalEntityAddress.AlternativePostalAddress.GeographicCoordinates.Longitude,
                            data.LegalEntityAddress.AlternativePostalAddress.GeographicCoordinates.Latitude,
                            data.LegalEntityAddress.AlternativePostalAddress.GeographicCoordinates.Altitude
                        ) : null,
                        data.LegalEntityAddress.AlternativePostalAddress.Country != null ? new BpdmCountryData(
                            data.LegalEntityAddress.AlternativePostalAddress.Country.TechnicalKey,
                            data.LegalEntityAddress.AlternativePostalAddress.Country.Name
                        ) : null,
                        data.LegalEntityAddress.AlternativePostalAddress.PostalCode,
                        data.LegalEntityAddress.AlternativePostalAddress.City,
                        data.LegalEntityAddress.AlternativePostalAddress.AdministrativeAreaLevel1 != null ? new BpdmAdministrativeAreaLevelData(
                            data.LegalEntityAddress.AlternativePostalAddress.AdministrativeAreaLevel1.CountryCode,
                            data.LegalEntityAddress.AlternativePostalAddress.AdministrativeAreaLevel1.RegionName,
                            data.LegalEntityAddress.AlternativePostalAddress.AdministrativeAreaLevel1.RegionCode
                        ) : null,
                        data.LegalEntityAddress.AlternativePostalAddress.DeliveryServiceNumber,
                        data.LegalEntityAddress.AlternativePostalAddress.DeliveryServiceType,
                        data.LegalEntityAddress.AlternativePostalAddress.DeliveryServiceQualifier
                    ) : null,
                    data.LegalEntityAddress.ConfidenceCriteria != null ? new BpdmConfidenceCriteriaData(
                        data.LegalEntityAddress.ConfidenceCriteria.SharedByOwner,
                        data.LegalEntityAddress.ConfidenceCriteria.CheckedByExternalDataSource,
                        data.LegalEntityAddress.ConfidenceCriteria.NumberOfSharingMembers,
                        data.LegalEntityAddress.ConfidenceCriteria.LastConfidenceCheckAt,
                        data.LegalEntityAddress.ConfidenceCriteria.NextConfidenceCheckAt,
                        data.LegalEntityAddress.ConfidenceCriteria.ConfidenceLevel
                    ) : null,
                    data.LegalEntityAddress.IsCatenaXMemberData
                ) : null
            ));
}
