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
                 EURprice = 15.59m
             },
             new Product()
             {
                 Name = "Bote chicles hierro",
                 Description = "Chicles de limón con relleno ácido",
                 Image = "images/hierro.png",
                 Stock = 300,
                 EURprice = 15.59m
             },
             new Product()
             {
                 Name = "Bote chicles diamante",
                 Description = "Chicles de menta con relleno menta",
                 Image = "images/diamante.png",
                 Stock = 300,
                 EURprice = 15.59m
             },
             new Product()
             {
                 Name = "Bote chicles esmeralda",
                 Description = "Chicles de sabor hierbabuena",
                 Image = "images/esmeralda.png",
                 Stock = 300,
                 EURprice = 12.59m
             },
             new Product()
             {
                 Name = "Bote chicles lapislazuli",
                 Description = "Chicles de sabor arándanos",
                 Image = "images/lapislazuli.png",
                 Stock = 300,
                 EURprice = 12.59m
             },
             new Product()
             {
                 Name = "Bote chicles obsidiana",
                 Description = "Chicles de sabor regaliz",
                 Image = "images/obsidiana.png",
                 Stock = 300,
                 EURprice = 12.59m
             },
             new Product()
             {
                 Name = "200 unidades chicles redstone",
                 Description = "Chicles de fresa con relleno ácido",
                 Image = "images/redstone.png",
                 Stock = 300,
                 EURprice = 11m
             },
             new Product()
             {
                 Name = "200 unidades chicles hierro",
                 Description = "Chicles de limón con relleno ácido",
                 Image = "images/hierro.png",
                 Stock = 300,
                 EURprice = 11m
             },
             new Product()
             {
                 Name = "200 unidades chicles diamante",
                 Description = "Chicles de menta con relleno menta",
                 Image = "images/diamante.png",
                 Stock = 300,
                 EURprice = 11m
             },
             new Product()
             {
                 Name = "200 unidades chicles esmeralda",
                 Description = "Chicles de sabor hierbabuena",
                 Image = "images/esmeralda.png",
                 Stock = 300,
                 EURprice = 10m
             },
             new Product()
             {
                 Name = "200 unidades chicles lapislazuli",
                 Description = "Chicles de sabor arándanos",
                 Image = "images/lapislazuli.png",
                 Stock = 300,
                 EURprice = 10m
             },
             new Product()
             {
                 Name = "200 unidades chicles obsidiana",
                 Description = "Chicles de sabor regaliz",
                 Image = "images/obsidiana.png",
                 Stock = 300,
                 EURprice = 10m
             }
         ];
        await _dbcontext.Products.AddRangeAsync(products);
    }
}


