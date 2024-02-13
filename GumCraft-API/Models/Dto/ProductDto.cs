namespace GumCraft_API.Models.Dto
{
    public class ProductDto
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Stock { get; set; }
        public decimal EURprice { get; set; }
        public decimal ETHprice { get; set; }
    }
}
