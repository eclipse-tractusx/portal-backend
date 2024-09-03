/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;

public class ProcessDbContext<TProcessTypeId, TProcessStepTypeId> : DbContext
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    protected ProcessDbContext()
    {
        throw new InvalidOperationException("IdentityService should never be null");
    }

    public ProcessDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public virtual DbSet<Process<TProcessTypeId, TProcessStepTypeId>> Processes { get; set; } = default!;
    public virtual DbSet<ProcessStep<TProcessTypeId, TProcessStepTypeId>> ProcessSteps { get; set; } = default!;
    public virtual DbSet<ProcessStepStatus<TProcessTypeId, TProcessStepTypeId>> ProcessStepStatuses { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessStep<TProcessTypeId, TProcessStepTypeId>>()
            .HasOne(d => d.Process)
            .WithMany(p => p.ProcessSteps)
            .HasForeignKey(d => d.ProcessId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ProcessStepStatus<TProcessTypeId, TProcessStepTypeId>>()
            .HasData(
                Enum.GetValues(typeof(ProcessStepStatusId))
                    .Cast<ProcessStepStatusId>()
                    .Select(e => new ProcessStepStatus<TProcessTypeId, TProcessStepTypeId>(e))
            );
    }
}
