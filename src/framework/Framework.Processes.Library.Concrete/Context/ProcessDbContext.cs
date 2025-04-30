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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Context;

public class ProcessDbContext<TProcess, TProcessTypeId>(DbContextOptions options) :
    DbContext(options),
    IProcessDbContext<TProcess, ProcessStep<TProcess>>,
    IDbContext
    where TProcess : class, IProcess, IProcessNavigation<ProcessStep<TProcess>>
    where TProcessTypeId : struct, Enum
{
    public virtual DbSet<TProcess> Processes { get; set; } = null!;
    public virtual DbSet<ProcessStep<TProcess>> ProcessSteps { get; set; } = null!;
    public virtual DbSet<ProcessStepStatus<TProcess>> ProcessStepStatuses { get; set; } = null!;
    public virtual DbSet<ProcessStepType<TProcess, ProcessType<ProcessStep<TProcess>>>> ProcessStepTypes { get; set; } = null!;
    public virtual DbSet<ProcessType<ProcessStep<TProcess>>> ProcessTypes { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TProcess>(p =>
        {
            p.ToTable("processes");
        });

        modelBuilder.Entity<ProcessStep<TProcess>>(ps =>
        {
            ps.HasOne(d => d.ProcessType)
                .WithMany(p => p.ProcessSteps)
                .HasForeignKey(d => d.ProcessTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            ps.HasOne(d => d.Process)
                .WithMany(p => p.ProcessSteps)
                .HasForeignKey(d => d.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            ps.HasOne(d => d.ProcessStepStatus)
                .WithMany(p => p.ProcessSteps)
                .HasForeignKey(d => d.ProcessStepStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            ps.HasOne(d => d.ProcessStepType)
                .WithMany(p => p.ProcessSteps)
                .HasForeignKey(d => new { d.ProcessStepTypeId, d.ProcessTypeId })
                .OnDelete(DeleteBehavior.ClientSetNull);

            ps.ToTable("process_steps");
        });

        modelBuilder.Entity<ProcessStepType<TProcess, ProcessType<ProcessStep<TProcess>>>>(pss =>
        {
            pss.HasKey(x => new { x.ProcessStepTypeId, x.ProcessTypeId });
            pss.HasData(
                Enum.GetValues<TProcessTypeId>()
                    .SelectMany(processTypeId => 
                        Enum.GetValues(processTypeId.GetLinkedProcessStepTypeIdType())
                            .Cast<Enum>()
                            .Select(pst =>
                            {
                                var executableSteps = processTypeId.GetExecutableProcessStepTypeIdsForProcessType();
                                var processStepTypeId = Convert.ToInt32(pst);
                                return new ProcessStepType<TProcess, ProcessType<ProcessStep<TProcess>>>(processStepTypeId, Convert.ToInt32(processTypeId), pst.ToString(), executableSteps.Contains(processStepTypeId));
                            }))
            );
            
            pss.ToTable("process_step_types");
        });

        modelBuilder.Entity<ProcessType<ProcessStep<TProcess>>>(pss =>
        {
            pss.HasData(
                Enum.GetValues<TProcessTypeId>()
                    .Select(pst => new ProcessType<ProcessStep<TProcess>>(Convert.ToInt32(pst), pst.ToString()))
            );
            
            pss.ToTable("process_types");
        });

        modelBuilder.Entity<ProcessStepStatus<TProcess>>(pss =>
        {
            pss.HasData(
                Enum.GetValues<ProcessStepStatusId>()
                    .Select(e => new ProcessStepStatus<TProcess>(e))
            );

            pss.ToTable("process_step_statuses");
        });
    }
}
