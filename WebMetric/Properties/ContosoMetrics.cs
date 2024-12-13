using System.Diagnostics.Metrics;

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