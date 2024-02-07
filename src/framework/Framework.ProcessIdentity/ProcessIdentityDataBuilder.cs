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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity;

public class ProcessIdentityDataBuilder : IProcessIdentityDataBuilder
{
    private readonly ProcessIdentitySettings _settings;
    private IdentityTypeId? _identityTypeId;
    private Guid? _companyId;

    public ProcessIdentityDataBuilder(IOptions<ProcessIdentitySettings> options)
    {
        _settings = options.Value;
    }

    public void AddIdentityData(IdentityTypeId identityType, Guid companyId)
    {
        _identityTypeId = identityType;
        _companyId = companyId;
    }

    public Guid IdentityId => _settings.ProcessUserId;

    public IdentityTypeId IdentityTypeId => _identityTypeId ?? throw new UnexpectedConditionException("identityType should never be null here (GetIdentityData must be called before)");

    public Guid CompanyId => _companyId ?? throw new UnexpectedConditionException("companyId should never be null here (GetIdentityData must be called before)");

}
