/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// Data Object for the App Release Details
/// </summary>
/// <param name="name"></param>
/// <param name="thumbnailUrl"></param>
/// <param name="provider"></param>
/// <param name="salesManagerId"></param>
/// <param name="providerCompanyId"></param>
/// <param name="companyName"></param>
/// <param name="appStatusId"></param>
/// <param name="lastChanged"></param>
/// <param name="description"></param>
/// <returns></returns>
public record AppReleaseData(string? name, string? thumbnailUrl, Guid? salesManagerId, Guid? providerCompanyId,string companyName, bool descriptionLong, bool descriptionShort);