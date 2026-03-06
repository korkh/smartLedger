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

            // 1. СВЯЗЬ КЛИЕНТ - ТРАНЗАКЦИИ (Один-ко-многим)
            builder
                .Entity<Client>()
                .HasMany(x => x.Transactions)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. СВЯЗЬ КЛИЕНТ - ТАРИФ (Строго Один-к-одному)
            // Это позволит тебе делать .Include(c => c.CurrentTariff) в Dashboard
            builder
                .Entity<Client>()
                .HasOne(c => c.CurrentTariff)
                .WithOne(t => t.Client)
                .HasForeignKey<ClientTariff>(t => t.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. ТОЧНОСТЬ ДЕСЯТИЧНЫХ ЧИСЕЛ (Важно для налогов и НДС)
            builder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.ExtraServiceAmount).HasPrecision(18, 2);
            });

            builder.Entity<ServiceReference>(entity =>
            {
                entity.Property(e => e.BasePrice).HasPrecision(18, 2);
                entity.Property(e => e.AffectsNdsThreshold).HasDefaultValue(false);
            });

            builder.Entity<ClientTariff>(entity =>
            {
                entity.Property(e => e.MonthlyFee).HasPrecision(18, 2);
                // УДАЛИЛ ТУТ ПОВТОРНУЮ КОНФИГУРАЦИЮ СВЯЗИ .HasOne().WithMany()
            });

            // 4. СИДИРОВАНИЕ РОЛЕЙ
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
