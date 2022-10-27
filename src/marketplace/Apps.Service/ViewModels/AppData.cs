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

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.ViewModels;

/// <summary>
/// View model of an application's base data.
/// </summary>
/// <param name="Id">Id of the App.</param>
/// <param name="Name">Name of the app.</param>
/// <param name="ShortDescription">Short description.</param>
/// <param name="Provider">Provider.</param>
/// <param name="Price">Price.</param>
/// <param name="LeadPictureUri">Lead pircture URI.</param>
/// <param name="UseCases">The apps use cases.</param>
public record AppData(
    Guid Id,
    string Name,
    string ShortDescription,
    string Provider,
    string Price,
    string LeadPictureUri,
    IEnumerable<string> UseCases);
