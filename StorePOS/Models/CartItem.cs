namespace StorePOS.Models;

public class CartItem
{
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    
    public decimal Total => Product.Price * Quantity;
}
