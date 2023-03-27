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

using System.ComponentModel.DataAnnotations;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;

/// <summary>
/// Model for requesting creation of an application.
/// </summary>
public class AppInputModel
{   
     /// <summary>
    /// Private constructor.
    /// </summary>
    private AppInputModel()
    {
        Provider = string.Empty;
        Price = string.Empty;
        UseCaseIds = new HashSet<Guid>();
        Descriptions = new HashSet<LocalizedDescription>();
        SupportedLanguageCodes = new HashSet<string>();
    }
    
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provider">Provider of the app.</param>
    /// <param name="price">Price of the app.</param>
    public AppInputModel(string provider, string price): this()
    {
        Provider = provider;
        Price = price;
    }

    /// <summary>
    /// Title or name of the app.
    /// </summary>
    [MaxLength(255)]
    public string? Title { get; set; }

    /// <summary>
    /// Provider of the app.
    /// </summary>
    [MaxLength(255)]
    public string Provider { get; set; }

    /// <summary>
    /// Uri to provider's marketing presence.
    /// </summary>
    [MaxLength(255)]
    public string? ProviderUri { get; set; }

    /// <summary>
    /// Uri for app access.
    /// </summary>
    [MaxLength(255)]
    public string? AppUri { get; set; }

    /// <summary>
    /// Uri to app's lead picture.
    /// </summary>
    [MaxLength(255)]
    public string? LeadPictureUri { get; set; }

    /// <summary>
    /// Email address of the app's primary contact.
    /// </summary>
    [MaxLength(255)]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Phone number of the app's primary contact.
    /// </summary>
    [MaxLength(255)]
    public string? ContactNumber { get; set; }

    /// <summary>
    /// ID of the app's providing company.
    /// </summary>
    public Guid? ProviderCompanyId { get; set; }

    /// <summary>
    /// ID of the app's sales manager.
    /// </summary>
    public Guid? SalesManagerId { get; set; }

    /// <summary>
    /// IDs of app's use cases.
    /// </summary>
    public virtual ICollection<Guid> UseCaseIds { get; set; }

    /// <summary>
    /// Descriptions of the app in different languages.
    /// </summary>
    public virtual ICollection<LocalizedDescription> Descriptions { get; set; }

    /// <summary>
    /// Two character language codes for the app's supported languages.
    /// </summary>
    public ICollection<string> SupportedLanguageCodes { get; set; }

    /// <summary>
    /// Pricing information of the app.
    /// </summary>
    public string Price { get; set; }
}
