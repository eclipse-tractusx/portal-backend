/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

public record UserCreationInfo(

    [property:JsonPropertyName("userName")]
    string? userName,

    [RegularExpression(@"^(([^<>()[\]\\.,;:\s@""]+(\.[^<>()[\]\\.,;:\s@""]+)*)|("".+""))@((\[\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\])|(([a-z0-9-]+\.)+[a-z]{2,}))$",
     ErrorMessage = "Invalid email")]
    [property:JsonPropertyName("email")]
    string eMail,

    [RegularExpression(@"^(([A-Za-zÀ-ÿ]{1,40}?([-,.'\s]?[A-Za-zÀ-ÿ]{1,40}?)){1,8})$",
     ErrorMessage = "Invalid firstName")]
    [property:JsonPropertyName("firstName")]
    string? firstName,

    [RegularExpression(@"^(([A-Za-zÀ-ÿ]{1,40}?([-,.'\s]?[A-Za-zÀ-ÿ]{1,40}?)){1,8})$",
     ErrorMessage = "Invalid lastName")]
    [property:JsonPropertyName("lastName")]
    string? lastName,

    [property:JsonPropertyName("roles")]
    IEnumerable<string> Roles
);

public record UserCreationInfoWithMessage(

    [property:JsonPropertyName("userName")]
    string? userName,

    [property:JsonPropertyName("email")]
    string eMail,

    [property:JsonPropertyName("firstName")]
    string? firstName,

    [property:JsonPropertyName("lastName")]
    string? lastName,

    [property:JsonPropertyName("roles")]
    IEnumerable<string> Roles,

    [property:JsonPropertyName("message")]
    string? Message
);
