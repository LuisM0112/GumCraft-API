namespace GumCraft_API.Models.Database.Entities;

public class Order
{
    public long OrderId { get; set; }

    //Para la clave foránea, relacion 1 a 1, un pedido, un usuario
    public User User { get; set; }
    public string Status { get; set; }
    public DateTime Date { get; set; }
    public decimal EURprice { get; set; }
    public decimal ETHtotal { get; set; }


    //Relación uno a muchos pedido-productoPedido
    public ICollection<ProductOrder> ProductsOrders { get; set; }
}
