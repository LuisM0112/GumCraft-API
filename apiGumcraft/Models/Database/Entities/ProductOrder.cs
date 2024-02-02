namespace apiGumcraft.Database.Entities
{
    public class ProductOrder
    {
        public long ProductOrderId { get; set; }

        //Claves foráneas
        public Order Order { get; set; }
        public Product Product { get; set; }
        public int amount { get; set; }
    }
}
