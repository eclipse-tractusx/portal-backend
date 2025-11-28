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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Next.Entities;

public class ProcessStepStatus<TProcess>(ProcessStepStatusId id) : IProcessStepStatus
    where TProcess : class, IProcess
{
    public ProcessStepStatusId Id { get; private set; } = id;

    [MaxLength(255)]
    public string Label { get; private set; } = id.ToString();

    // Navigation properties
    public virtual ICollection<ProcessStep<TProcess>> ProcessSteps { get; private set; } = new HashSet<ProcessStep<TProcess>>();
}
