using apiGumcraft.Database.Entities;
using GumcraftApi.Models.Database.Entities;

namespace apiGumcraft.Controllers
{
    public class ProductCartDto
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }

        public decimal Price { get; set; }
    }

}