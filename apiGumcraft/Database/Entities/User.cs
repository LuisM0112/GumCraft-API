using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace apiGumcraft.Database.Entities
{
    [Index(nameof(Email),IsUnique = true)]
    public class User
    {

        public long UserId { get; set; }
        public string Name { get; set; }

        
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }

        public string TransactionHash { get; set; }
        public string WalletAddress { get; set; }

        //Relación 1 a muchos, un usuario muchos pedidos
        public ICollection<Order> Orders { get; set; }
    }
}
