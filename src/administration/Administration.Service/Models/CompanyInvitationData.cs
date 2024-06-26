/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

public class CompanyInvitationData
{
    public CompanyInvitationData(string? userName,
        string firstName,
        string lastName,
        string email,
        string organisationName)
    {
        UserName = userName;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        OrganisationName = organisationName;
    }

    [JsonPropertyName("userName")]
    public string? UserName { get; init; }

    [DefaultValue("string")]
    [RegularExpression(ValidationExpressions.Name, ErrorMessage = "Invalid firstName", MatchTimeoutInMilliseconds = 500)]
    [JsonPropertyName("firstName")]
    public string FirstName { get; init; }

    [DefaultValue("string")]
    [RegularExpression(ValidationExpressions.Name, ErrorMessage = "Invalid lastName", MatchTimeoutInMilliseconds = 500)]
    [JsonPropertyName("lastName")]
    public string LastName { get; init; }

    [DefaultValue("string")]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; init; }

    [JsonPropertyName("organisationName")]
    public string OrganisationName { get; init; }

    public void Deconstruct(out string? userName, out string firstName, out string lastName, out string email, out string organisationName)
    {
        userName = UserName;
        firstName = FirstName;
        lastName = LastName;
        email = Email;
        organisationName = OrganisationName;
    }
}
