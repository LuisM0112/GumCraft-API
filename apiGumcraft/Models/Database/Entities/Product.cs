using GumcraftApi.Models.Database.Entities;

namespace apiGumcraft.Database.Entities;

public class Product
{
    public long ProductId {  get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Image {  get; set; }
    public int Stock { get; set; }
    public int EURprice { get; set; }
    public int ETHprice {  get; set; }

    //Relación uno a muchos producto-productoCarrito
    public ICollection<ProductCart> ProductsCart { get; set; }

    //Relación uno a mucho producto-pedidoProducto
    public ICollection<ProductOrder> ProductsOrder { get; set; }

}
