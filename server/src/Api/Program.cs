using Api.App;
using Application.Logging;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Starting EnBref server...");

builder.WebHost.ConfigureKestrel(options =>
{
    // remove the Server header for security reasons
    options.AddServerHeader = false;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.ConfigureLogging();
builder.Services.AddInfrastructureBlocks(isUsingDocker: !builder.Environment.IsDevelopment());

Console.WriteLine("Loading modules...");
builder.Services.LoadModules();
builder.Services.LoadConfigurations(builder);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders
app.UseSecurityHeaders();

app.MapControllers();

// Sonde du healthcheck Docker : volontairement sans dépendance (pas de LiteDB, pas
// d'appel sortant), elle ne répond que « le process sert des requêtes ».
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

Console.WriteLine("Application started.");
app.Run();
