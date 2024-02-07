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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.ProcessIdentity;

public class ProcessIdentityDataDetermination : IProcessIdentityDataDetermination
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IProcessIdentityDataBuilder _processIdentityDataBuilder;

    public ProcessIdentityDataDetermination(IPortalRepositories portalRepositories, IProcessIdentityDataBuilder processIdentityDataBuilder)
    {
        _identityRepository = portalRepositories.GetInstance<IIdentityRepository>();
        _processIdentityDataBuilder = processIdentityDataBuilder;
    }

    /// <inheritdoc />
    public async Task GetIdentityData()
    {
        (IdentityTypeId IdentityTypeId, Guid CompanyId) identityData;

        if ((identityData = await _identityRepository.GetActiveIdentityDataByIdentityId(_processIdentityDataBuilder.IdentityId).ConfigureAwait(false)) == default)
            throw new ConflictException($"Identity {_processIdentityDataBuilder.IdentityId} could not be found");

        _processIdentityDataBuilder.AddIdentityData(identityData.IdentityTypeId, identityData.CompanyId);
    }
}
