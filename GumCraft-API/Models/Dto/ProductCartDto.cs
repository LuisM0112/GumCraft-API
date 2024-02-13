using GumCraft_API.Database.Entities;
using GumCraft_API.Models.Database.Entities;

namespace GumCraft_API.Controllers
{
    public class ProductCartDto
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }

        public decimal Price { get; set; }
    }

}