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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

public class ClaimsIdentityService : IIdentityService
{
    private readonly IIdentityData _identityData;
    public ClaimsIdentityService(IClaimsIdentityDataBuilder claimsIdentityDataBuilder)
    {
        _identityData = claimsIdentityDataBuilder;
    }

<<<<<<<< HEAD:src/framework/Framework.Web/ClaimsIdentityService.cs
    public IIdentityData IdentityData => _identityData;
========
    /// <inheritdoc />
    public IdentityData IdentityData =>
        _identityData ??= _httpContextAccessor.HttpContext?.User.GetIdentityData()
                          ?? throw new ConflictException("The identity should be set here");
>>>>>>>> c2ab69a7f (feat(nuget): create framework and framework.web nuget packages):src/web/Web.Identity/IdentityService.cs
}
