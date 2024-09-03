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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;

public class Process<TProcessTypeId, TProcessStepTypeId> : IBaseEntity, ILockableEntity
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    private Process()
    {
        ProcessSteps = new HashSet<ProcessStep<TProcessTypeId, TProcessStepTypeId>>();
    }

    public Process(Guid id, TProcessTypeId processTypeId, Guid version) : this()
    {
        Id = id;
        ProcessTypeId = processTypeId;
        Version = version;
    }

    public Guid Id { get; private set; }

    public TProcessTypeId ProcessTypeId { get; set; }

    public DateTimeOffset? LockExpiryDate { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; }

    public virtual ICollection<ProcessStep<TProcessTypeId, TProcessStepTypeId>> ProcessSteps { get; private set; }
}
