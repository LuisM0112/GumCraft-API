﻿namespace GumCraft_API.Models.Database.Entities;

public class Product
{
    public long ProductId {  get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Image {  get; set; }
    public int Stock { get; set; }
    public decimal EURprice { get; set; }

    //Relación uno a muchos producto-productoCarrito
    public ICollection<ProductCart> ProductsCart { get; set; }

    //Relación uno a mucho producto-pedidoProducto
    public ICollection<ProductOrder> ProductsOrder { get; set; }

}
