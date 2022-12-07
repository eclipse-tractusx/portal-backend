/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.ProvisioningEntities
{
    public partial class ProvisioningDBContext : DbContext
    {
        public ProvisioningDBContext()
        {
        }

        public ProvisioningDBContext(DbContextOptions<ProvisioningDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ClientSequence> ClientSequences { get; set; }
        public virtual DbSet<IdentityProviderSequence> IdentityProviderSequences { get; set; }
        public virtual DbSet<UserPasswordReset> UserPasswordResets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");

            modelBuilder.Entity<ClientSequence>(entity =>
            {
                entity.HasKey(e => e.SequenceId)
                    .HasName("client_sequence_pkey");

                entity.ToTable("client_sequence", "provisioning");

                entity.Property(e => e.SequenceId)
                    .HasColumnName("sequence_id")
                    .HasDefaultValueSql("nextval('client_sequence_sequence_id_seq'::regclass)");
            });

            modelBuilder.Entity<IdentityProviderSequence>(entity =>
            {
                entity.HasKey(e => e.SequenceId)
                    .HasName("identity_provider_sequence_pkey");

                entity.ToTable("identity_provider_sequence", "provisioning");

                entity.Property(e => e.SequenceId)
                    .HasColumnName("sequence_id")
                    .HasDefaultValueSql("nextval('identity_provider_sequence_sequence_id_seq'::regclass)");
            });

            modelBuilder.Entity<UserPasswordReset>(entity =>
            {
                entity.ToTable("user_password_resets", "provisioning");

                entity.Property(e => e.UserEntityId)
                    .HasColumnName("user_entity_id");

                entity.Property(e => e.PasswordModifiedAt)
                    .HasColumnName("password_modified_at")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.ResetCount)
                    .HasColumnName("reset_count")
                    .HasDefaultValue(0);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
