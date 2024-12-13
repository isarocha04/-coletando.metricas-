
# Implementação\ Métricas com Prometheus e Grafana

## Introdução
Este guia mostra um passo a passo de como implementar a coleta de métricas em uma aplicação ASP.NET Core utilizando OpenTelemetry, Prometheus e o Grafana.

---

## Configuração 

### Estrutura inicial
**Criação da aplicação**
   ```bash
   dotnet new web -o WebMetric
   cd WebMetric
   ```
**Adicionar pacotes necessários**
   ```bash
   dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore --prerelease
   dotnet add package OpenTelemetry.Extensions.Hosting
   ```

---

### Configuração do Código
 **Classe `ContosoMetrics.cs`**
  - Implementar a classe com o contador para rastrear vendas
   ```csharp
   public class ContosoMetrics
   {
       private static readonly Meter Meter = new("Contoso.Web");
       private readonly Counter<int> _productSoldCounter;

       public ContosoMetrics()
       {
           _productSoldCounter = Meter.CreateCounter<int>("contoso.product.sold");
       }

       public void ProductSold(string productName, int quantity)
       {
           _productSoldCounter.Add(quantity,
               new KeyValuePair<string, object?>("contoso.product.name", productName));
       }
   }
   ```

**Modelo de dados `SaleModel.cs`**
   Estrutura básica do modelo:
   ```csharp
   public class SaleModel
   {
       public string ProductName { get; set; } = string.Empty;
       public int QuantitySold { get; set; } = 0;
   }
   ```

 **Configuração no `Program.cs`**
   Adicionar as configurações para OpenTelemetry e Prometheus
   ```csharp
   using Microsoft.AspNetCore.Http.Features;
   using OpenTelemetry.Metrics;

   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddOpenTelemetry()
       .WithMetrics(metricsBuilder =>
       {
           metricsBuilder.AddPrometheusExporter();
           metricsBuilder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
       });

   var app = builder.Build();

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

   app.MapPrometheusScrapingEndpoint();
   app.MapPost("/complete-sale", (SaleModel model, ContosoMetrics metrics) =>
   {
       metrics.ProductSold(model.ProductName, model.QuantitySold);
       return Results.Ok("Venda registrada com sucesso!");
   });

   app.Run();
   ```

---

## Configuração do Prometheus

### Arquivo `prometheus.yml`
- Configurar o arquivo com o endpoint da aplicação
   ```yaml
   global:
     scrape_interval: 15s

   scrape_configs:
     - job_name: 'MyASPNETApp'
       static_configs:
         - targets: ['webmetrics:5045']
   ```

### Iniciar
**Executar o Prometheus com Docker** 
   ```bash
   docker run -p 9090:9090 -v $(pwd)/prometheus.yml:/etc/prometheus/prometheus.yml prom/prometheus
   ```
   
---

## Configuração do Grafana
- Configurar uma fonte de dados Prometheus no Grafana.
-  Criar gráficos para visualizar as métricas.
![alt text](<Captura de Tela 2024-12-13 às 16.45.42.png>)
---

## Execução e Testes

### **Execução da Aplicação:**
   - Rode o projeto:
     ```bash
     dotnet run
     ```

### **Acesso às Métricas:**
   - Naveguar para `http://webmetric:5045/metrics`.

### **Gráficos no Grafana:**
   - Acessar o painel e visualize os dados em tempo real.

