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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;

public class ProcessStep<TProcess>(
    Guid id,
    int processTypeId,
    int processStepTypeId,
    ProcessStepStatusId processStepStatusId,
    Guid processId,
    DateTimeOffset dateCreated) :
    IProcessStep,
    IProcessStepNavigation<TProcess, ProcessType<ProcessStep<TProcess>>, ProcessStepType<TProcess, ProcessType<ProcessStep<TProcess>>>>,
    IBaseEntity
    where TProcess : class, IProcess
{
    public Guid Id { get; private set; } = id;

    public int ProcessTypeId { get; set; } = processTypeId;

    public int ProcessStepTypeId { get; private set; } = processStepTypeId;

    public ProcessStepStatusId ProcessStepStatusId { get; set; } = processStepStatusId;

    public Guid ProcessId { get; private set; } = processId;

    public DateTimeOffset DateCreated { get; private set; } = dateCreated;

    public DateTimeOffset? DateLastChanged { get; set; }

    public string? Message { get; set; }

    // Navigation properties
    public virtual TProcess? Process { get; private set; }
    public virtual ProcessType<ProcessStep<TProcess>>? ProcessType { get; private set; }
    public virtual ProcessStepType<TProcess, ProcessType<ProcessStep<TProcess>>>? ProcessStepType { get; private set; }
    public virtual ProcessStepStatus<TProcess>? ProcessStepStatus { get; private set; }
}
