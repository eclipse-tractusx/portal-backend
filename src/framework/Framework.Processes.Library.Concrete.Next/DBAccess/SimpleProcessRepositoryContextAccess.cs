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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Next.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Next.DBAccess;

public class SimpleProcessRepositoryContextAccess<TProcessDbContext>(TProcessDbContext dbContext) :
    IProcessRepositoryContextAccess<SimpleProcess, ProcessType<ProcessStep<SimpleProcess>>, ProcessStep<SimpleProcess>, ProcessStepType<SimpleProcess, ProcessType<ProcessStep<SimpleProcess>>>>
    where TProcessDbContext : class, IProcessDbContext<SimpleProcess, ProcessStep<SimpleProcess>>, IDbContext
{
    public DbSet<SimpleProcess> Processes => dbContext.Processes;
    public DbSet<ProcessStep<SimpleProcess>> ProcessSteps => dbContext.ProcessSteps;

    public SimpleProcess CreateProcess(Guid id, Guid version) =>
        new(id, version);

    public ProcessStep<SimpleProcess> CreateProcessStep<TProcessTypeId, TProcessStepTypeId>(Guid id, TProcessTypeId processTypeId, TProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId, DateTimeOffset now) =>
        new(id, Convert.ToInt32(processTypeId), Convert.ToInt32(processStepTypeId), processStepStatusId, processId, now);
}
