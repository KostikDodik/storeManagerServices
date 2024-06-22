using Microsoft.Extensions.Configuration;
using Model.Database;

namespace Data;

using Microsoft.EntityFrameworkCore;

internal class DataDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            optionsBuilder.UseSqlServer();
        }
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Supply> Supplies { get; set; }
    public DbSet<SupplyRow> SupplyRows { get; set; }
    
    public DbSet<SalePlatform> SalePlatforms { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    public DbSet<Item> Items { get; set; }
    
}