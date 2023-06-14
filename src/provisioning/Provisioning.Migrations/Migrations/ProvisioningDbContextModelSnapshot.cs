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

// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.ProvisioningEntities;

#nullable disable

namespace Org.Eclipse.TractusX.Provisioning.Migrations.Migrations
{
    [DbContext(typeof(ProvisioningDbContext))]
    partial class ProvisioningDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("provisioning")
                .UseCollation("en_US.utf8")
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.HasSequence<int>("client_sequence_sequence_id_seq");

            modelBuilder.HasSequence<int>("identity_provider_sequence_sequence_id_seq");

            modelBuilder.Entity("Org.Eclipse.TractusX.Portal.Backend.Provisioning.ProvisioningEntities.ClientSequence", b =>
                {
                    b.Property<int>("SequenceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("sequence_id")
                        .HasDefaultValueSql("nextval('provisioning.client_sequence_sequence_id_seq'::regclass)");

                    b.HasKey("SequenceId")
                        .HasName("pk_client_sequences");

                    b.ToTable("client_sequences", "provisioning");
                });

            modelBuilder.Entity("Org.Eclipse.TractusX.Portal.Backend.Provisioning.ProvisioningEntities.IdentityProviderSequence", b =>
                {
                    b.Property<int>("SequenceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("sequence_id")
                        .HasDefaultValueSql("nextval('provisioning.identity_provider_sequence_sequence_id_seq'::regclass)");

                    b.HasKey("SequenceId")
                        .HasName("pk_identity_provider_sequences");

                    b.ToTable("identity_provider_sequences", "provisioning");
                });

            modelBuilder.Entity("Org.Eclipse.TractusX.Portal.Backend.Provisioning.ProvisioningEntities.UserPasswordReset", b =>
                {
                    b.Property<Guid>("CompanyUserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("company_user_id");

                    b.Property<DateTimeOffset>("PasswordModifiedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("password_modified_at");

                    b.Property<int>("ResetCount")
                        .HasColumnType("integer")
                        .HasColumnName("reset_count");

                    b.HasKey("CompanyUserId")
                        .HasName("pk_user_password_resets");

                    b.ToTable("user_password_resets", "provisioning");
                });
#pragma warning restore 612, 618
        }
    }
}
