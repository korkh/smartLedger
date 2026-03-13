using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Storage
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../API"))
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<DataContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlite(connectionString);

            // Создаем заглушку, так как во время дизайна (миграций) пользователя нет
            var stubUserAccessor = new DesignTimeUserAccessor();

            return new DataContext(builder.Options, stubUserAccessor);
        }
    }

    // Вспомогательный класс-заглушка
    public class DesignTimeUserAccessor : IUserAccessor
    {
        public string GetUserName() => "System_Migration";

        public bool IsAdmin() => true;

        public bool IsSeniorAccountant() => true;

        public int GetUserId() => 0;
    }
}
