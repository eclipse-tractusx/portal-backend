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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Identities;

public class IdentityService : IIdentityService
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IIdentityIdDetermination _identityIdDetermination;
    private IdentityData? _identityData;

    public IdentityService(IPortalRepositories portalRepositories, IIdentityIdDetermination identityIdDetermination)
    {
        _identityRepository = portalRepositories.GetInstance<IIdentityRepository>();
        _identityIdDetermination = identityIdDetermination;
    }

    /// <inheritdoc />
    public async ValueTask<IdentityData> GetIdentityData() =>
        _identityData ??= await _identityRepository.GetActiveIdentityDataByIdentityId(IdentityId).ConfigureAwait(false) ?? throw new ConflictException($"Identity {_identityIdDetermination.IdentityId} could not be found");

    public IdentityData IdentityData => _identityData ?? throw new UnexpectedConditionException("identityData should never be null here (endpoint must be annotated with an identity policy / as an alternative GetIdentityData should be used)");

    public Guid IdentityId => _identityIdDetermination.IdentityId;
}
