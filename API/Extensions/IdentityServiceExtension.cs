using API.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Storage;

namespace API.Extensions
{
    public static class IdentityServiceExtension
    {
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            /**
                    * TOKEN SERVICE
            **/
            services
                .AddIdentity<User, Role>(opt =>
                {
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<DataContext>()
                .AddSignInManager<SignInManager<User>>()
                .AddDefaultTokenProviders();

            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(config["TokenKey"])
            );

            /**
                    * AUTHENTICATION
            **/
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    };
                    opt.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(
                                new { error = "У вас нет доступа!" }
                            );
                            return context.Response.WriteAsync(result);
                        },
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (
                                !string.IsNullOrEmpty(accessToken)
                                && path.StartsWithSegments("/chat")
                            )
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                    };
                });

            /**

                * AUTHORIZATION

            **/

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "Level1Only",
                    policy =>
                    {
                        policy.RequireRole("Junior_Accountant");
                    }
                );

                options.AddPolicy(
                    "Level2Only",
                    policy =>
                    {
                        policy.RequireRole("Senior_Accountant");
                    }
                );

                options.AddPolicy(
                    "Level3Only",
                    policy =>
                    {
                        policy.RequireRole("Admin");
                    }
                );
            });

            services.AddScoped<TokenService>();

            return services;
        }
    }
}
