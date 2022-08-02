﻿/********************************************************************************
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

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Provides access to methods to work with Countries
/// </summary>
public interface ICountryRepository
{
    /// <summary>
    /// Checks if a country exists in the database depending on the alpha2code
    /// </summary>
    /// <param name="alpha2Code">The alpha2code to check the countries for</param>
    /// <returns><c>true</c> if the country exists for the given alpha 2 code, otherwise <c>false</c></returns>
    Task<bool> CheckCountryExistsByAlpha2CodeAsync(string alpha2Code);
}
