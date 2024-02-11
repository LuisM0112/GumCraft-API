using apiGumcraft.Database.Entities;
using GumcraftApi.Models.Database.Entities;

namespace apiGumcraft.Controllers
{
    public class CartDto
    {
        public long CartId { get; set; }
        public long UserId { get; set; }
        public ICollection<ProductCart> ProductsCart { get; set; }
    }
}