using GumCraft_API.Database;
using GumCraft_API.Models.Database.Entities;

namespace GumCraft_API.Models.Database;

public class DbSeeder
{
    private readonly MyDbContext _dbcontext;

    public DbSeeder(MyDbContext DbContext)
    {
        _dbcontext = DbContext;
    }
    public async Task SeedAsync()
    {
        bool created = await _dbcontext.Database.EnsureCreatedAsync();

        if (created)
        {
            await SeedProductsAsync();
        }
        _dbcontext.SaveChanges();
    }

    private async Task SeedProductsAsync()
    {
        Product[] products =
        [
            new Product()
            {
                Name = "Bote chicles redstone",
                Description = "Chicles de fresa con relleno ácido",
                Image = "images/redstone.png",
                Stock = 300,
                EURprice = 3.99m
            }
        ];
        await _dbcontext.Products.AddRangeAsync(products);
    }
}


