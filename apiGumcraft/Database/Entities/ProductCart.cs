namespace apiGumcraft.Database.Entities
{

    public class ProductCart
    {
        public long ProductCartId { get; set; }
        public Cart CartId { get; set; }
        public Product Product { get; set; }
        public int Amount { get; set; }
    }
}