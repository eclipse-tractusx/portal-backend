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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Handles the read access to the languages
/// </summary>
public class LanguageRepository : ILanguageRepository
{
    private readonly PortalDbContext _portalDbContext;

    /// <summary>
    /// Initializes a new instance of <see cref="LanguageRepository"/>
    /// </summary>
    /// <param name="portalDbContext">Access to the database</param>
    public LanguageRepository(PortalDbContext portalDbContext)
    {
        _portalDbContext = portalDbContext;
    }

    /// <inheritdoc />
    public Task<string?> GetLanguageAsync(string languageShortName) =>
        _portalDbContext.Languages
            .AsNoTracking()
            .Where(language => language.ShortName == languageShortName)
            .Select(language => language.ShortName)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<string> GetLanguageCodesUntrackedAsync(IEnumerable<string> languageCodes) =>
        _portalDbContext.Languages.AsNoTracking()
            .Where(x => languageCodes.Contains(x.ShortName))
            .Select(x => x.ShortName)
            .AsAsyncEnumerable();
}
