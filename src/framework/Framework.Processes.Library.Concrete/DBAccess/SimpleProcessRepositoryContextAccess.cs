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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.DBAccess;

public class SimpleProcessRepositoryContextAccess<TProcessTypeId, TProcessStepTypeId, TProcessDbContext>(TProcessDbContext dbContext) :
    IProcessRepositoryContextAccess<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, ProcessType<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId>, ProcessStep<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, ProcessStepType<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
    where TProcessDbContext : class, IProcessDbContext<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, ProcessType<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId>, ProcessStep<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>, IDbContext
{
    public DbSet<SimpleProcess<TProcessTypeId, TProcessStepTypeId>> Processes => dbContext.Processes;
    public DbSet<ProcessStep<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>> ProcessSteps => dbContext.ProcessSteps;

    public SimpleProcess<TProcessTypeId, TProcessStepTypeId> CreateProcess(Guid id, TProcessTypeId processTypeId, Guid version) => new(id, processTypeId, version);
    public ProcessStep<SimpleProcess<TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId> CreateProcessStep(Guid id, TProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId, DateTimeOffset now) => new(id, processStepTypeId, processStepStatusId, processId, now);
}
