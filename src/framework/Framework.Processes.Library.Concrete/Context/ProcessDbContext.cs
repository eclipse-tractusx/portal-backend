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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Context;

public class ProcessDbContext<TProcess, TProcessTypeId, TProcessStepTypeId>(DbContextOptions options) :
    DbContext(options),
    IProcessDbContext<TProcess, ProcessType<TProcess, TProcessTypeId>, ProcessStep<TProcess, TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>,
    IDbContext
    where TProcess : class, IProcess<TProcessTypeId>, IProcessNavigation<ProcessType<TProcess, TProcessTypeId>, ProcessStep<TProcess, TProcessTypeId, TProcessStepTypeId>, TProcessTypeId, TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    public virtual DbSet<TProcess> Processes { get; set; } = default!;
    public virtual DbSet<ProcessStep<TProcess, TProcessTypeId, TProcessStepTypeId>> ProcessSteps { get; set; } = default!;
    public virtual DbSet<ProcessStepStatus<TProcess, TProcessTypeId, TProcessStepTypeId>> ProcessStepStatuses { get; set; } = default!;
    public virtual DbSet<ProcessStepType<TProcess, TProcessTypeId, TProcessStepTypeId>> ProcessStepTypes { get; set; } = default!;
    public virtual DbSet<ProcessType<TProcess, TProcessTypeId>> ProcessTypes { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TProcess>(p =>
        {
            p.HasOne(d => d.ProcessType)
                .WithMany(p => p!.Processes)
                .HasForeignKey(d => d.ProcessTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            p.ToTable("processes");
        });

        modelBuilder.Entity<ProcessStep<TProcess, TProcessTypeId, TProcessStepTypeId>>(ps =>
        {
            ps.HasOne(d => d.Process)
                .WithMany(p => p.ProcessSteps)
                .HasForeignKey(d => d.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            ps.HasOne(d => d.ProcessStepStatus)
                .WithMany(p => p.ProcessSteps)
                .HasForeignKey(d => d.ProcessStepStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            ps.ToTable("process_steps");
        });

        modelBuilder.Entity<ProcessType<TProcess, TProcessTypeId>>()
            .HasData(
                Enum.GetValues(typeof(TProcessTypeId))
                    .Cast<TProcessTypeId>()
                    .Select(e => new ProcessType<TProcess, TProcessTypeId>(e))
            );

        modelBuilder.Entity<ProcessStepType<TProcess, TProcessTypeId, TProcessStepTypeId>>()
            .HasData(
                Enum.GetValues(typeof(TProcessStepTypeId))
                    .Cast<TProcessStepTypeId>()
                    .Select(e => new ProcessStepType<TProcess, TProcessTypeId, TProcessStepTypeId>(e))
            );

        modelBuilder.Entity<ProcessStepStatus<TProcess, TProcessTypeId, TProcessStepTypeId>>(pss =>
        {
            pss.HasData(
                Enum.GetValues(typeof(ProcessStepStatusId))
                    .Cast<ProcessStepStatusId>()
                    .Select(e => new ProcessStepStatus<TProcess, TProcessTypeId, TProcessStepTypeId>(e))
            );

            pss.ToTable("process_step_statuses");
        });
    }
}
