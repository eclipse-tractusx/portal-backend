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

 
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

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
}
