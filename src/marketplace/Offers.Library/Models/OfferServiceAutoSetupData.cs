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

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;

/// <summary>
/// Data object to retrieve the needed data to auto setup an service
/// </summary>
/// <param name="RequestId">Id of the offer subscription</param>
/// <param name="OfferUrl">Url of the service</param>
public record OfferAutoSetupData(Guid RequestId, string OfferUrl);

/// <summary>
/// Response data for the service auto setup
/// </summary>
/// <param name="TechnicalUserInfo">Object containing the information of the technical user</param>
/// <param name="ClientInfo">Information of the created client</param>
public record OfferAutoSetupResponseData(TechnicalUserInfoData? TechnicalUserInfo, ClientInfoData? ClientInfo);

/// <summary>
/// Technical User information
/// </summary>
/// <param name="TechnicalUserId">Id of the created technical user</param>
/// <param name="TechnicalUserSecret">User secret for the created user</param>
/// <param name="TechnicalClientId">User secret for the created user</param>
public record TechnicalUserInfoData(Guid TechnicalUserId, string? TechnicalUserSecret, string? TechnicalClientId);

/// <summary>
/// Client infos
/// </summary>
/// <param name="ClientId">Id of the created client</param>
public record ClientInfoData(string ClientId);
