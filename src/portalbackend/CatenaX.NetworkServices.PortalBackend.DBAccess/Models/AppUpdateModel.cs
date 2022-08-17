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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

/// <summary>
/// DTO to update an app
/// </summary>
public class AppUpdateModel
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="descriptions"></param>
    /// <param name="images"></param>
    /// <param name="providerUri"></param>
    /// <param name="contactEmail"></param>
    /// <param name="contactNumber"></param>
    public AppUpdateModel(IEnumerable<AppDescription> descriptions, IEnumerable<AppDetailImage> images, string? providerUri, string? contactEmail, string? contactNumber)
    {
        Descriptions = descriptions;
        Images = images;
        ProviderUri = providerUri;
        ContactEmail = contactEmail;
        ContactNumber = contactNumber;
    }
    public IEnumerable<AppDescription> Descriptions { get; set; }
    public IEnumerable<AppDetailImage> Images { get; set; }
    public string? ProviderUri { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactNumber { get; set; }
}






