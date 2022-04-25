using Microsoft.EntityFrameworkCore;

#nullable disable

namespace CatenaX.NetworkServices.Provisioning.ProvisioningEntities
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

                entity.Property(e => e.SharedUserEntityId)
                    .HasColumnName("shared_user_entity_id");

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
