using System.Text.Json.Serialization;
using EnBref.Api.BackOffice;
using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Features.GenerateRecap;
using EnBref.Api.Shared;
using EnBref.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCollectHeadlines();
builder.Services.AddGenerateRecap();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "EnBref"));
app.UseAntiforgery();
app.MapStaticAssets();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = (context, report) => context.Response.WriteAsJsonAsync(
        new { status = report.Status.ToString(), version = ServerVersion.Current }),
});
app.MapGenerateRecap();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
