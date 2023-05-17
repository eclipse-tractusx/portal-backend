/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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
	public partial class ProvisioningDbContext : DbContext
	{
		protected ProvisioningDbContext()
		{
		}

		public ProvisioningDbContext(DbContextOptions<ProvisioningDbContext> options)
			: base(options)
		{
		}

		public virtual DbSet<ClientSequence> ClientSequences { get; set; }
		public virtual DbSet<IdentityProviderSequence> IdentityProviderSequences { get; set; }
		public virtual DbSet<UserPasswordReset> UserPasswordResets { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSnakeCaseNamingConvention();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");
			modelBuilder.HasDefaultSchema("provisioning");

			modelBuilder.HasSequence<int>("client_sequence_sequence_id_seq", "provisioning");
			modelBuilder.HasSequence<int>("identity_provider_sequence_sequence_id_seq", "provisioning");

			modelBuilder.Entity<ClientSequence>(entity =>
			{
				entity.HasKey(e => e.SequenceId);

				entity.Property(e => e.SequenceId)
					.HasDefaultValueSql("nextval('provisioning.client_sequence_sequence_id_seq'::regclass)");
			});

			modelBuilder.Entity<IdentityProviderSequence>(entity =>
			{
				entity.HasKey(e => e.SequenceId);

				entity.Property(e => e.SequenceId)
					.HasDefaultValueSql("nextval('provisioning.identity_provider_sequence_sequence_id_seq'::regclass)");
			});
		}
	}
}
