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

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;

/// <summary>
/// Model for Input Role
/// </summary>
/// <param name="Role"></param>
/// <param name="Descriptions"></param>
/// <returns></returns>
public record AppUserRole(string Role, IEnumerable<AppUserRoleDescription> Descriptions);

/// <summary>
/// Model for Role Description
/// </summary>
/// <param name="LanguageCode"></param>
/// <param name="Description"></param>
/// <returns></returns>
public record AppUserRoleDescription(string LanguageCode, string Description);

/// <summary>
/// Model for Role Data
/// </summary>
/// <param name="RoleId"></param>
/// <param name="RoleName"></param>
/// <returns></returns>
public record AppRoleData(Guid RoleId, string RoleName);

