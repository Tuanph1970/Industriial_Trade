using IndustryTrade.Api;
using IndustryTrade.BuildingBlocks.Application.Behaviors;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.BuildingBlocks.Web.Security;
using IndustryTrade.Modules.Catalog.Api;
using IndustryTrade.Modules.Analytics.Api;
using IndustryTrade.Modules.AuditSystem.Api;
using IndustryTrade.Modules.IdentityAccess.Api;
using IndustryTrade.Modules.Integration.Api;
using IndustryTrade.Modules.Notifications.Api;
using IndustryTrade.Modules.Reporting.Api;
using IndustryTrade.Modules.SectorData.Api;
using MediatR;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging -------------------------------------------------------------
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ---- Modules (bounded contexts) ------------------------------------------
// Register every context here. The host composes them; modules stay decoupled.
IModule[] modules =
[
    new IdentityAccessModule(),
    new CatalogModule(),
    new SectorDataModule(),
    new ReportingModule(),
    new NotificationsModule(),
    new AnalyticsModule(),
    new AuditSystemModule(),
    new IntegrationModule(),
];

foreach (var module in modules)
    module.RegisterServices(builder.Services, builder.Configuration);

// ---- Security (Keycloak OIDC + current user) -----------------------------
builder.Services.AddKeycloakAuth(builder.Configuration);

// ---- CQRS pipeline (executed in registration order) ----------------------
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>)); // innermost: records command + outcome

// ---- Web -----------------------------------------------------------------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var scheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = [] });
});
builder.Services.AddHealthChecks();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration["Cors:Origins"]?.Split(',') ?? ["http://localhost:5173"])
     .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Dev convenience: apply each module's migrations on startup.
    // In production, run migrations as an explicit deploy step instead.
    foreach (var module in modules)
        await module.ApplyMigrationsAsync(app.Services);
}

app.MapHealthChecks("/health");

foreach (var module in modules)
    module.MapEndpoints(app);

app.Run();
