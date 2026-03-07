using Application.Clients;
using Application.Common.Configurations;
using Application.Common.Interfaces;
using Application.Core;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Storage;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            // 1. Контроллеры, авторизация по умолчанию и JSON
            services
                .AddControllers(opt =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    opt.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddJsonOptions(options =>
                {
                    // Полезно для работы с Guid и Enum на фронтенде
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            services.AddOpenApi();

            services.AddLocalization(opt => opt.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "nb-NO", "ru-RU", "en-US" };
                options
                    .SetDefaultCulture(supportedCultures[0])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });

            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            // });

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            // POSTGRESQL

            // services.AddDbContext<DataContext>(options =>
            // {
            //     var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //     string connStr;

            //     // Depending on if in development or production, use either FlyIO
            //     // connection string, or development connection string from env var.
            //     if (env == "Development")
            //     {
            //         // Use connection string from file.
            //         connStr = config.GetConnectionString("DefaultConnection");
            //     }
            //     else
            //     {
            //         // Use connection string provided at runtime by FlyIO.
            //         var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            //         // Parse connection URL to connection string for Npgsql
            //         connUrl = connUrl.Replace("postgres://", string.Empty);
            //         var pgUserPass = connUrl.Split("@")[0];
            //         var pgHostPortDb = connUrl.Split("@")[1];
            //         var pgHostPort = pgHostPortDb.Split("/")[0];
            //         var pgDb = pgHostPortDb.Split("/")[1];
            //         var pgUser = pgUserPass.Split(":")[0];
            //         var pgPass = pgUserPass.Split(":")[1];
            //         var pgHost = pgHostPort.Split(":")[0];
            //         var pgPort = pgHostPort.Split(":")[1];
            //         var updatedHost = pgHost.Replace("flycast", "internal");

            //         connStr =
            //             $"Server={updatedHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
            //     }

            //     options.UseNpgsql(connStr);
            // });

            services.AddCors(options =>
            {
                options.AddPolicy(
                    "CorsPolicy",
                    policy =>
                    {
                        policy
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithExposedHeaders("WWW-Authenticate", "Pagination")
                            .WithOrigins("http://localhost:3000", "https://localhost:3000");
                    }
                );
            });

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(GetList.Handler).Assembly)
            );
            // Assembly allocates all mapping profiles inside the project
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CreateClient>();

            // Services for relationships
            services.AddHttpContextAccessor();
            services.AddAutoMapper(cfg => { }, typeof(MappingProfiles).Assembly);
            services.AddScoped<IUserAccessor, UserAccessor>();

            // Services
            // services.AddScoped<IPhotoAccessor, PhotoAccessor>();
            // services.AddScoped<EmailSender>();
            services.AddScoped<ISearchExpressionBuilder, SearchExpressionBuilder>();
            services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));

            return services;
        }
    }
}
