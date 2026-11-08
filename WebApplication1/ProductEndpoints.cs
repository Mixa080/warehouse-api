namespace WebApplication1;
using Microsoft.EntityFrameworkCore;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products");
        
        group.MapGet("/{id:guid}", async (Guid id, WarehouseDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        }).RequireAuthorization();
        group.MapGet("/", async (WarehouseDbContext db) =>
        {
            var products = await db.Products.ToListAsync();
            return Results.Ok(products);
        }).RequireAuthorization();
        
        group.MapPost("/", async (CreateProductDto dto, WarehouseDbContext db, ProductValidators vd) =>
        {
            var valid = await vd.ValidateAsync(dto);
            if (!valid.IsValid)
            {
                return Results.ValidationProblem(valid.ToDictionary());
            }

            var newProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Sku = dto.Sku,
                Price = dto.Price,
                Quantity = dto.QuantityInStock,
                CreatedAt = DateTime.UtcNow
            };

            db.Products.Add(newProduct);
            await db.SaveChangesAsync();
            return Results.Created($"/products/{newProduct.Id}", newProduct);
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductDto dto, WarehouseDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
            {
                return Results.NotFound();
            }

            product.Name = dto.Name;
            product.Sku = dto.Sku;
            product.Price = dto.Price;
            product.Quantity = dto.QuantityInStock; 

            await db.SaveChangesAsync();
            return Results.Ok(product);
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, WarehouseDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
            {
                return Results.NotFound();
            }

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.Ok(product);
        }).RequireAuthorization();
        return app;
    }
}