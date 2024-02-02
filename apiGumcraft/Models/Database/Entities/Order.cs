using GumcraftApi.Models.Database.Entities;

namespace apiGumcraft.Database.Entities;

public class Order
{
    public long OrderId { get; set; }

    //Para la clave foránea, relacion 1 a 1, un pedido, un usuario
    public User User { get; set; }
    public string Status { get; set; }
    public DateTime Date { get; set; }
    public int EURprice { get; set; }
    public int ETHtotal { get; set; }


    //Relación uno a muchos pedido-productoPedido
    public ICollection<ProductOrder> ProductsOrders { get; set; }
}
