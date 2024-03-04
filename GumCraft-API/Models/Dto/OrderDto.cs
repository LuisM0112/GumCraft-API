namespace GumCraft_API.Models.Dto
{
    public class OrderDto
    {
        public long OrderId { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public decimal EURprice { get; set; }
        public decimal ETHtotal { get; set; }
    }
}
