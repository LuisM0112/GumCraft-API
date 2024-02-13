using GumCraft_API.Database.Entities;
using GumCraft_API.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace GumCraft_API.Database;

public class MyDbContext: DbContext
{
    private const string DATABASE_PATH = "gumcraft.db";
    public DbSet<User> Users { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCart> ProductsCart { get; set; }
    public DbSet<ProductOrder> ProdcutsOrder { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        options.UseSqlite($"DataSource={baseDir}{DATABASE_PATH}");
    }

}
