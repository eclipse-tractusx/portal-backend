/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.DBAccess;

public interface IProcessCreation<out TProcess, TProcessType, out TProcessStep, TProcessStepType>
    where TProcess : class, IProcess, IProcessNavigation<TProcessStep>
    where TProcessType : class, IProcessType, IProcessTypeNavigation<TProcessStep>
    where TProcessStep : class, IProcessStep, IProcessStepNavigation<TProcess, TProcessType, TProcessStepType>
    where TProcessStepType : class, IProcessStepType, IProcessStepTypeNavigation<TProcessStep, TProcessType>
{
    TProcess CreateProcess(Guid id, Guid version);
    TProcessStep CreateProcessStep<TProcessTypeId, TProcessStepTypeId>(Guid id, TProcessTypeId processTypeId, TProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId, DateTimeOffset now);
}
