using WebApplication1;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidators>();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
     app.MapGet("/products", async (WarehouseDbContext db) =>
     {
         var product = await db.Products.ToListAsync();
         return Results.Ok(product);
     });
app.MapGet("/products/{id:guid}", async (Guid id,WarehouseDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});
app.MapPost("/products", async (CreateProductDto dto,WarehouseDbContext db, ProductValidators vd) =>
{
    var valid = await vd.ValidateAsync(dto);
    if (!valid.IsValid)
    {
        return Results.ValidationProblem(valid.ToDictionary());
    }
    var newProduct = new Product()
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
});
app.MapDelete("/products/{id:guid}", async (Guid id,WarehouseDbContext db) =>
{
    var product = await db.Products.FindAsync(id);

    if (product is null)
    {
        return Results.NotFound();
    }
 
    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.Ok(product);
});
app.MapPut("/product/{id:guid}", async (UpdateProductDto dto,Guid id, WarehouseDbContext db) =>
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
});
app.Run();
