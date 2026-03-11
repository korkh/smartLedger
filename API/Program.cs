using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Все сервисы (DB, CORS, MediatR, Controllers, Auth, Localization) теперь внутри этих двух строк
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// 1. Error Handling & Security
// Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
app.UseXContentTypeOptions();
app.UseReferrerPolicy(options => options.NoReferrer()); // Referrer-Policy controls how much information about the referring page is sent with navigation requests.
app.UseXXssProtection(Options => Options.EnabledWithBlockMode());
app.UseXfo(opt => opt.Deny()); //against click-hijacking
app.UseCsp(opt =>
    opt //all from wwwroot from client folder are allowed and for development to remove all issues use app.UseCspReportOnly but before production just app.UseCsp
    .BlockAllMixedContent()
        .StyleSources(s =>
            s.Self()
                .CustomSources(
                    "https://fonts.googleapis.com",
                    "sha256-47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU="
                )
        )
        .FontSources(s => s.Self().CustomSources("https://fonts.gstatic.com", "data:"))
        .FormActions(s => s.Self())
        .FrameAncestors(s => s.Self())
        .ImageSources(s => s.Self().CustomSources("blob:", "data:", "https://res.cloudinary.com"))
);

//Add in developing mode Swagger and Swagger UI middleware to enable API documentation and exploration.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.Use(
        async (context, next) =>
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000"); // 1 year
            // context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=()");
            await next.Invoke();
        }
    );
}

// 3. Routing & Auth
app.UseCors("CorsPolicy");
app.UseRequestLocalization(); // Теперь вызывается без параметров, т.к. они в сервисах
app.UseAuthentication();
app.UseAuthorization();

// 4. Static files & Controllers
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

// app.MapFallbackToController("Index", "Fallback");

// Initialize Database (Migrations + Seed)
await DbInitializer.InitDb(app);

app.Run();
