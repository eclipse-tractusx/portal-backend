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

namespace CatenaX.NetworkServices.Apps.Service.ViewModels;

/// <summary>
/// View model of an application's base data.
/// </summary>
public class AppData
{
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="title">Title.</param>
    /// <param name="shortDescription">Short description.</param>
    /// <param name="provider">Provider.</param>
    /// <param name="price">Price.</param>
    /// <param name="leadPictureUri">Lead pircture URI.</param>
    public AppData(string title, string shortDescription, string provider, string price, string leadPictureUri)
    {
        Title = title;
        ShortDescription = shortDescription;
        Provider = provider;
        UseCases = new List<string>();
        Price = price;
        LeadPictureUri = leadPictureUri;
    }

    public Guid Id { get; init; }
    public string Title { get; init; }
    public string ShortDescription { get; init; }
    public string Provider { get; init; }
    public string Price { get; init; }
    public string LeadPictureUri { get; init; }
    public IEnumerable<string> UseCases { get; init; }

}
