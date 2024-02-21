namespace GumCraft_API.Models.Database.Entities;

public class Transaction
{
    public int Id { get; set; }
    public string ClientWallet { get; set; }
    public string Value { get; set; }
    public string Hash { get; set; }
    public bool Completed {  get; set; }
}
