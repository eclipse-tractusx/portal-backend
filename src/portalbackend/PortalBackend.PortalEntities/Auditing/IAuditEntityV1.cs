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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;

/// <summary>
/// Marker interface to define that the entity is an audit entity
/// </summary>
/// <remarks>
/// The implementation of this Attribute must not be changed.
/// When changes are needed create a V2 of it.
/// </remarks>
public interface IAuditEntityV1
{
	/// <summary>
	/// Id of the audited entity
	/// </summary>
	Guid AuditV1Id { get; set; }

	/// <summary>
	/// Date Time of the last change of the entity
	/// </summary>
	DateTimeOffset AuditV1DateLastChanged { get; set; }

	/// <summary>
	/// Reference to the <see cref="CompanyUser"/> that changed the entity
	/// </summary>
	Guid? AuditV1LastEditorId { get; set; }

	/// <summary>
	/// Id of the audit operation
	/// </summary>
	AuditOperationId AuditV1OperationId { get; set; }
}
