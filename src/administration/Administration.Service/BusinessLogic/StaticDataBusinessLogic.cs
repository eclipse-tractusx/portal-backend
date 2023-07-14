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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IStaticDataBusinessLogic"/> making use of <see cref="IStaticDataRepository"/> to retrieve data.
/// </summary>
public class StaticDataBusinessLogic : IStaticDataBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalRepositories"></param>
    public StaticDataBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<UseCaseData> GetAllUseCase() =>
        _portalRepositories.GetInstance<IStaticDataRepository>().GetAllUseCase();

    /// <inheritdoc/>
    public IAsyncEnumerable<LanguageData> GetAllLanguage() =>
        _portalRepositories.GetInstance<IStaticDataRepository>().GetAllLanguage();

    /// <inheritdoc/>
    public IAsyncEnumerable<LicenseTypeData> GetAllLicenseType() =>
        _portalRepositories.GetInstance<IStaticDataRepository>().GetLicenseTypeData();

    /// <inheritdoc />
    public IAsyncEnumerable<OperatorBpnData> GetOperatorBpns() =>
        _portalRepositories.GetInstance<ICompanyRepository>().GetOperatorBpns();
}
