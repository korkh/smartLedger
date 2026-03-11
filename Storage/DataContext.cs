using Domain.Entities;
using Domain.Entities.Common;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Storage
{
    public class DataContext(DbContextOptions<DataContext> options, IUserAccessor userAccessor)
        : IdentityDbContext<User, Role, int>(options)
    {
        private readonly IUserAccessor _userAccessor = userAccessor;

        public DbSet<Client> Clients { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ServiceReference> ServiceReferences { get; set; }
        public DbSet<ClientTariff> ClientTariffs { get; set; }
        public DbSet<UserTask> Tasks { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Игнорируем проверку изменений модели при каждом запуске
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning)
            );
        }

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
                        // Можно добавить поле LastModifiedBy в BaseEntity по аналогии
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Глобальный фильтр: по умолчанию загружаем только не удаленные записи
            builder.Entity<Client>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<ClientTariff>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<ServiceReference>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<UserTask>().HasQueryFilter(u => !u.IsDeleted);

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
