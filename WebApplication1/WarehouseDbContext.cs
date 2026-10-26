using Microsoft.EntityFrameworkCore;

namespace WebApplication1;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
}