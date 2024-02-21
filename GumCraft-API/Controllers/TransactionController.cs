using GumCraft_API.APIs;
using GumCraft_API.Database;
using GumCraft_API.Database.Entities;
using GumCraft_API.Models.Classes;
using GumCraft_API.Models.Database.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.Web3;
using System.Numerics;

namespace GumCraft_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private const string OUR_WALLET = "0xBFC126F6d19cbbE6AE54B62DcC54e5F98dA3ee76";
        private const string NETWORK_URL = "https://rpc.sepolia.org";

        private readonly MyDbContext _dbContext;

        public TransactionController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("buy/{productId}")]
        public async Task<IActionResult> BuyAsync([FromBody] string clientWallet)
        {
            IActionResult statusCode;
            string cartID = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.CartId.ToString().Equals(cartID));

            if(cart == null)
            {
                statusCode = NotFound("Carrito no encontrado");
            }
            else
            {
                var total = cart.ProductsCart.Sum(pc => pc.Product.EURprice * pc.Amount);

                using CoinGeckoApi coinGeckoApi = new CoinGeckoApi();
                decimal ethereumEur = await coinGeckoApi.GetEthereumPriceAsync();
                BigInteger priceWei = Web3.Convert.ToWei(total / ethereumEur); //Wei

                Web3 web3 = new Web3(NETWORK_URL);

                TransactionToSign transactionToSing = new TransactionToSign()
                {
                    From = clientWallet,
                    To = OUR_WALLET,
                    Value = new HexBigInteger(priceWei).HexValue,
                    Gas = new HexBigInteger(30000).HexValue,
                    GasPrice = (await web3.Eth.GasPrice.SendRequestAsync()).HexValue
                };

                Transaction transaction = new Transaction()
                {
                    Id = _dbContext.Transactions.Count,
                    ClientWallet = transactionToSing.From,
                    Value = transactionToSing.Value
                };

                _dbContext.Transactions.Add(transaction);
                transactionToSing.Id = transaction.Id;

                statusCode = Ok(transactionToSing);
            }

            return statusCode;
        }

        [HttpPost("check/{transactionId}")]
        public async Task<bool> CheckTransactionAsync(int transactionId, [FromBody] string txHash)
        {
            bool success = false;
            Transaction transaction = _dbContext.Transactions[transactionId];
            transaction.Hash = txHash;

            Web3 web3 = new Web3(NETWORK_URL);
            var receiptPollingService = new TransactionReceiptPollingService(
                web3.TransactionManager, 1000);

            try
            {
                // Esperar a que la transacción se confirme en la cadena de bloques
                var transactionReceipt = await receiptPollingService.PollForReceiptAsync(txHash);

                // Obtener los datos de la transacción
                var transactionEth = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);

                Console.WriteLine(transactionEth.TransactionHash == transactionReceipt.TransactionHash);
                Console.WriteLine(transactionReceipt.Status.Value == 1);
                Console.WriteLine(transactionReceipt.TransactionHash == transaction.Hash);
                Console.WriteLine(transactionReceipt.From == transaction.ClientWallet);
                Console.WriteLine(transactionReceipt.To.Equals(OUR_WALLET, StringComparison.OrdinalIgnoreCase));

                success = transactionEth.TransactionHash == transactionReceipt.TransactionHash
                    && transactionReceipt.Status.Value == 1 // Transacción realizada con éxito
                    && transactionReceipt.TransactionHash == transaction.Hash // El hash es el mismo
                    && transactionReceipt.From == transaction.ClientWallet // El dinero viene del cliente
                    && transactionReceipt.To.Equals(OUR_WALLET, StringComparison.OrdinalIgnoreCase); // El dinero se ingresa en nuestra cuenta
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al esperar la transacción: {ex.Message}");
            }

            transaction.Completed = success;

            return success;
        }
    }
}
