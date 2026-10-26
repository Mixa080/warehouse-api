namespace WebApplication1;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
public record CreateProductDto(string Name, string Sku, decimal Price, int QuantityInStock);
public record UpdateProductDto(string Name, string Sku, decimal Price, int QuantityInStock);
