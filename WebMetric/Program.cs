using Microsoft.AspNetCore.Http.Features;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Registrar ContosoMetrics
builder.Services.AddSingleton<ContosoMetrics>();

// Configurar OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder.AddPrometheusExporter();
        metricsBuilder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
    });

builder.WebHost.UseUrls("http://*:5045");

var app = builder.Build();

// Rota Prometheus
app.MapPrometheusScrapingEndpoint();

// Middleware para enriquecer métricas
app.Use(async (context, next) =>
{
    var tagsFeature = context.Features.Get<IHttpMetricsTagsFeature>();
    if (tagsFeature != null)
    {
        var source = context.Request.Query["utm_medium"].ToString() switch
        {
            "" => "none",
            "social" => "social",
            "email" => "email",
            "organic" => "organic",
            _ => "other"
        };
        tagsFeature.Tags.Add(new KeyValuePair<string, object?>("mkt_medium", source));
    }

    await next.Invoke();
});

// Rota teste
app.MapGet("/", () => "Hello World!");

// Rota para registrar vendas
app.MapPost("/complete-sale", (SaleModel model, ContosoMetrics metrics) =>
{
    metrics.ProductSold(model.ProductName, model.QuantitySold);
    return Results.Ok("Venda registrada com sucesso!");
});

app.Run();
