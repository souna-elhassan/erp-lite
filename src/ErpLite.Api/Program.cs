using ErpLite.Api.Endpoints;
using ErpLite.Api.Infrastructure;
using ErpLite.Application;
using ErpLite.Infrastructure;
using ErpLite.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<ErpLiteDbContext>("database");

const string WebClientCors = "web-client";
builder.Services.AddCors(options => options.AddPolicy(WebClientCors, policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
          .AllowAnyHeader()
          .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Local dev and the docker-compose demo create the database and add fake demo data on startup.
// Both flags default to false: a real deployment applies migrations from a reviewed SQL script.
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ErpLiteDbContext>();
    await db.Database.MigrateAsync();
    if (app.Configuration.GetValue<bool>("Database:SeedDemoData"))
        await DevelopmentSeeder.SeedAsync(db);
}

app.UseCors(WebClientCors);

app.MapHealthChecks("/health");
app.MapProductEndpoints();

await app.RunAsync();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program;
