namespace GumCraft_API.Models.Database.Entities;

public class Cart
{
    public long CartId { get; set; }

    //Clave foránea y relación 1 a 1, un usuario un carrito
    public User User { get; set; }
        

    //Relación uno a muchos Carrito-ProductosCarrito
    public ICollection<ProductCart> ProductsCart { get; set; }
}
