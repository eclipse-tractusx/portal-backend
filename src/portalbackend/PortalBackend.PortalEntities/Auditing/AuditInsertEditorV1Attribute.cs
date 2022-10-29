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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;

/// <summary>
/// Attribute to mark the creators id in the base class.
/// The usage is optional. If not set <see cref="AuditLastEditorV1Attribute"/>
/// is being used to determine the creators id.
/// </summary>
/// <remarks>
/// The implementation of this Attribute must not be changed.
/// When changes are needed create a V2 of it.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class AuditInsertEditorV1Attribute : Attribute
{
}
