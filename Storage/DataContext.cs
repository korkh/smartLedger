using Domain.Entities;
using Domain.Entities.Common;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Storage
{
    public class DataContext : IdentityDbContext<User, Role, int>
    {
        private readonly IUserAccessor _userAccessor;

        public DataContext(DbContextOptions<DataContext> options, IUserAccessor userAccessor)
            : base(options)
        {
            _userAccessor = userAccessor;
        }

        // --- CORE ENTITIES ---
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientInternal> ClientInternals { get; set; }
        public DbSet<ClientSensitive> ClientSensitives { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ServiceReference> ServiceReferences { get; set; }

        public DbSet<ClientTariff> ClientTariffs { get; set; }
        public DbSet<TariffHistory> TariffHistories { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<UserTask> Tasks { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning)
            );
        }

        // ---------------------------------------------------------
        // SAVE CHANGES + SOFT DELETE
        // ---------------------------------------------------------
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default
        )
        {
            var currentUserName = _userAccessor.GetUserName() ?? "System";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUserName;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Deleted:
                        if (entry.Entity is ISoftDelete softDelete)
                        {
                            entry.State = EntityState.Modified;
                            softDelete.IsDeleted = true;
                            softDelete.DeletedAt = DateTime.UtcNow;
                        }
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        // ---------------------------------------------------------
        // MODEL CONFIGURATION
        // ---------------------------------------------------------
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ----------------------------------------------------
            // ENCRYPTION CONFIGURATION (через DI)
            // ----------------------------------------------------
            var sensitiveConfigs = this.GetService<
                IEnumerable<IEntityTypeConfiguration<ClientSensitive>>
            >();
            foreach (var config in sensitiveConfigs)
                builder.ApplyConfiguration(config);

            // --- GLOBAL SOFT DELETE FILTERS ---
            builder.Entity<Client>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<ClientTariff>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<ServiceReference>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<UserTask>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<TariffHistory>().HasQueryFilter(h => !h.IsDeleted);
            builder.Entity<Invoice>().HasQueryFilter(i => !i.IsDeleted);

            // -----------------------------------------------------
            // CLIENT → TRANSACTIONS (1:M)
            // -----------------------------------------------------
            builder
                .Entity<Client>()
                .HasMany(c => c.Transactions)
                .WithOne(t => t.Client)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------------------------------
            // CLIENT → TARIFFS (1:M)
            // -----------------------------------------------------
            builder
                .Entity<Client>()
                .HasMany(c => c.ClientTariffs)
                .WithOne(t => t.Client)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------------------------------
            // TARIFF HISTORY → CLIENT (M:1)
            // -----------------------------------------------------
            builder
                .Entity<TariffHistory>()
                .HasOne(h => h.Client)
                .WithMany()
                .HasForeignKey(h => h.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Entity<TariffHistory>()
                .HasOne(h => h.Tariff)
                .WithMany()
                .HasForeignKey(h => h.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------------------------------
            // INVOICE → CLIENT (M:1)
            // -----------------------------------------------------
            builder
                .Entity<Invoice>()
                .HasOne(i => i.Client)
                .WithMany()
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // -----------------------------------------------------
            // DECIMAL PRECISION
            // -----------------------------------------------------
            builder
                .Entity<Transaction>()
                .Property(e => e.ExtraServiceAmount)
                .HasPrecision(18, 2);

            builder.Entity<Transaction>().Property(e => e.NdsBaseAmount).HasPrecision(18, 2);

            builder.Entity<ServiceReference>().Property(e => e.BasePrice).HasPrecision(18, 2);

            builder.Entity<ClientTariff>().Property(e => e.MonthlyFee).HasPrecision(18, 2);

            builder.Entity<Invoice>().Property(e => e.TariffAmount).HasPrecision(18, 2);

            builder.Entity<Invoice>().Property(e => e.ExtraServicesAmount).HasPrecision(18, 2);

            builder.Entity<Invoice>().Property(e => e.OveruseAmount).HasPrecision(18, 2);

            // -----------------------------------------------------
            // SEED ROLES
            // -----------------------------------------------------
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
