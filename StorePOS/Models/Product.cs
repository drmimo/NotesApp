namespace StorePOS.Models;

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime AdditionDate { get; set; }
    public DateTime? EditDate { get; set; }
    public List<string> Barcodes { get; set; } = new();
}
