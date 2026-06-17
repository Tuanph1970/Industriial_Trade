using IndustryTrade.Api;
using IndustryTrade.BuildingBlocks.Application.Behaviors;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.IdentityAccess.Api;
using MediatR;
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
    // new CatalogModule(),
    // new SectorDataModule(),
    // new ReportingModule(),
    // new AnalyticsModule(),
    // new IntegrationModule(),
    // new AuditSystemModule(),
];

foreach (var module in modules)
    module.RegisterServices(builder.Services, builder.Configuration);

// ---- CQRS pipeline (executed in registration order) ----------------------
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ---- Web -----------------------------------------------------------------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration["Cors:Origins"]?.Split(',') ?? ["http://localhost:5173"])
     .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors();

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
