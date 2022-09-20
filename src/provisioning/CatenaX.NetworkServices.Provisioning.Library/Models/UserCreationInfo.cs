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

using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public class UserCreationInfo
{
    public UserCreationInfo(string? userName, string email, string? firstName, string? lastName, IEnumerable<string> roles, string? message)
    {
        this.userName = userName;
        this.eMail = email;
        this.firstName = firstName;
        this.lastName = lastName;
        this.Roles = roles;
        this.Message = message;
    }

    [JsonPropertyName("userName")]
    public string? userName { get; set; }

    [JsonPropertyName("email")]
    public string eMail { get; set; }

    [JsonPropertyName("firstName")]
    public string? firstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? lastName { get; set; }

    [JsonPropertyName("roles")]
    public IEnumerable<string> Roles { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
