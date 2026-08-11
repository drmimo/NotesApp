namespace StorePOS.Models;

public class Order
{
    public string Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public TimeSpan OrderTime { get; set; }
    public decimal Total { get; set; }
    public List<OrderProduct> Products { get; set; } = new();
}
