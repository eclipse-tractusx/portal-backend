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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// View model of an application's detailed data specific for service consents
/// </summary>
/// <param name="Id">ID of the service</param>
/// <param name="CompanyName">Name of the company that gave the consent</param>
/// <param name="CompanyUserId">ID of the company that gave the consent</param>
/// <param name="ConsentStatus">Consent Status</param>
/// <param name="AgreementName">The agreement name</param>
public record ConsentDetailData(
    Guid Id,
    string CompanyName,
    Guid CompanyUserId,
    ConsentStatusId ConsentStatus,
    string AgreementName);
