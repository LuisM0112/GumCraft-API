using GumCraft_API.Models.Database.Entities;

namespace GumCraft_API.Controllers
{
    public class CartDto
    {
        public long CartId { get; set; }
        public long UserId { get; set; }
        public ICollection<ProductCart> ProductsCart { get; set; }
    }
}