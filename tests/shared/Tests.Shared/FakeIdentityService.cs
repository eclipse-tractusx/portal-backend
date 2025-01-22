/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;

public class FakeIdentityService : IIdentityService
{
    private readonly Guid _identityId = new("ac1cf001-7fbc-1f2f-817f-bce058020001");

    /// <inheritdoc />
    public IIdentityData IdentityData =>
        new FakeIdentityData(_identityId, IdentityTypeId.COMPANY_USER, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"));

    private sealed record FakeIdentityData(Guid IdentityId, IdentityTypeId IdentityTypeId, Guid CompanyId) : IIdentityData;
}
