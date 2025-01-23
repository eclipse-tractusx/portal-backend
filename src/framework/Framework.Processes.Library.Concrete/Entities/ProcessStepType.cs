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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;

public class ProcessStepType<TProcess, TProcessTypeId, TProcessStepTypeId>(TProcessStepTypeId id) :
    IProcessStepType<TProcessStepTypeId>,
    IProcessStepTypeNavigation<ProcessStep<TProcess, TProcessTypeId, TProcessStepTypeId>, TProcessStepTypeId>
    where TProcess : class, IProcess<TProcessTypeId>, IProcessNavigation<ProcessType<TProcess, TProcessTypeId>, ProcessStep<TProcess, TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    public TProcessStepTypeId Id { get; private set; } = id;

    [MaxLength(255)]
    public string Label { get; private set; } = id.ToString()!;

    // Navigation properties
    public virtual ICollection<ProcessStep<TProcess, TProcessTypeId, TProcessStepTypeId>> ProcessSteps { get; private set; } = new HashSet<ProcessStep<TProcess, TProcessTypeId, TProcessStepTypeId>>();
}
