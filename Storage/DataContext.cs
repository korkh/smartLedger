using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    public class DataContext(DbContextOptions<DataContext> options)
        : IdentityDbContext<User, Role, int>(options)
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ServiceReference> ServiceReferences { get; set; }
        public DbSet<ClientTariff> ClientTariffs { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Client - Transactions relationship configuration
            builder
                .Entity<Client>()
                .HasMany(x => x.Transactions)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade); // Example: delete transactions if client is removed

            // Client - Tariffs relationship configuration
            builder.Entity<ClientTariff>().Property(p => p.MonthlyFee).HasPrecision(18, 2);

            // Tariffs - Services relationship configuration
            builder.Entity<ServiceReference>().Property(p => p.BasePrice).HasPrecision(18, 2);

            // Configuration for ServiceReference
            builder.Entity<ServiceReference>(entity =>
            {
                entity.Property(e => e.BasePrice).HasPrecision(18, 2);
                // Default value for the new flag
                entity.Property(e => e.AffectsNdsThreshold).HasDefaultValue(false);
            });

            // Configuration for ClientTariff
            builder.Entity<ClientTariff>(entity =>
            {
                entity.Property(e => e.MonthlyFee).HasPrecision(18, 2);
                // One-to-one or One-to-many relationship depending on your needs
                entity
                    .HasOne(d => d.Client)
                    .WithMany() // or WithOne(c => c.CurrentTariff)
                    .HasForeignKey(d => d.ClientId);
            });

            // Seeding roles
            builder
                .Entity<Role>()
                .HasData(
                    new Role
                    {
                        Id = 1,
                        Name = "Junior_Accountant",
                        NormalizedName = "JUNIOR_ACCOUNTANT",
                    },
                    new Role
                    {
                        Id = 2,
                        Name = "Senior_Accountant",
                        NormalizedName = "SENIOR_ACCOUNTANT",
                    },
                    new Role
                    {
                        Id = 3,
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                    }
                );
        }
    }
}
