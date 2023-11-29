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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

public static class PortalClaimTypes
{
    private const string Base = "https://catena-x.net//schema/2023/05/identity/claims";
    public const string Sub = "sub";
    public const string PreferredUserName = "preferred_username";
    public const string ResourceAccess = "resource_access";
    public const string CompanyId = $"{Base}/company_id";
    public const string IdentityId = $"{Base}/identity_id";
    public const string IdentityType = $"{Base}/identity_type";
}
