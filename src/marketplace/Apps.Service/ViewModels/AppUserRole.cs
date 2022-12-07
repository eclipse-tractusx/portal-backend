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

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;

/// <summary>
/// Model for Input Role
/// </summary>
/// <param name="role"></param>
/// <param name="descriptions"></param>
/// <returns></returns>
public record AppUserRole(string role, IEnumerable<AppUserRoleDescription> descriptions);

/// <summary>
/// Model for Role Description
/// </summary>
/// <param name="languageCode"></param>
/// <param name="description"></param>
/// <returns></returns>
public record AppUserRoleDescription(string languageCode, string description);

/// <summary>
/// Model for Role Data
/// </summary>
/// <param name="roleId"></param>
/// <param name="roleName"></param>
/// <returns></returns>
public record AppRoleData(Guid roleId, string roleName);

