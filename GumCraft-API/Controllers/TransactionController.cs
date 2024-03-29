﻿using GumCraft_API.APIs;
using GumCraft_API.Database;
using GumCraft_API.Models.Classes;
using GumCraft_API.Models.Database.Entities;
using GumCraft_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.Web3;
using System.Numerics;
using System.Text;

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

        [HttpPost("Buy")]
        public async Task<IActionResult> BuyAsync([FromBody] string clientWallet)
        {
            IActionResult statusCode;
            string userId = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.User.UserId.ToString().Equals(userId));

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

                TransactionToSign transactionToSign = new TransactionToSign()
                {
                    From = clientWallet,
                    To = OUR_WALLET,
                    Value = new HexBigInteger(priceWei).HexValue,
                    Gas = new HexBigInteger(30000).HexValue,
                    GasPrice = (await web3.Eth.GasPrice.SendRequestAsync()).HexValue
                };

                Transaction transaction = new Transaction()
                {
                    ClientWallet = transactionToSign.From,
                    Value = transactionToSign.Value
                };

                await _dbContext.Transactions.AddAsync(transaction);
                await _dbContext.SaveChangesAsync();

                transactionToSign.Id = transaction.TransactionId;

                statusCode = Ok(transactionToSign);
                
            }

            return statusCode;
        }

        [HttpPost("check/{transactionId}")]
        public async Task<bool> CheckTransactionAsync(int transactionId, [FromBody] string txHash)
        {
            string userId = User.FindFirst("id").Value;
            bool success = false;
            Transaction transaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId);
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

                if (success)
                {
                    await SaveCartOrder(userId);
                    await ClearCart(userId);

                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
            }

            transaction.Completed = success;

            

            return success;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SaveCartOrder(string userId)
        {
            IActionResult statusCode;

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId.ToString().Equals(userId));
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.User.UserId.ToString().Equals(userId));

            if (user == null)
            {
                statusCode = NotFound("Usuario no encontrado");
            }
            else if (cart == null)
            {
                statusCode = NotFound("Carrito no encontrado");
            }
            else
            {
                decimal totalEUR = cart.ProductsCart.Sum(pc => pc.Product.EURprice * pc.Amount);
                Console.WriteLine(totalEUR.ToString());
                using CoinGeckoApi coinGeckoApi = new CoinGeckoApi();
                decimal ethereumEur = await coinGeckoApi.GetEthereumPriceAsync();
                decimal priceETH = totalEUR / ethereumEur;

                Order newOrder = new Order()
                {
                    User = user,
                    Status = "PENDING",
                    Date = DateTime.Now,
                    EURprice = totalEUR,
                    ETHtotal = priceETH,
                    ProductsOrders = new List<ProductOrder>()
                };
                await _dbContext.Orders.AddAsync(newOrder);
                await _dbContext.SaveChangesAsync();

                var currentOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.User.UserId.ToString().Equals(userId));
                if (currentOrder == null)
                {
                    statusCode = NotFound("Pedido no encontrado");
                }
                else
                {
                    currentOrder.ProductsOrders = cart.ProductsCart.Select(pc => new ProductOrder()
                    {
                        Order = newOrder,
                        Product = pc.Product,
                        Amount = pc.Amount,
                    }).ToList();

                    await _dbContext.SaveChangesAsync();

                    var email = new emailService();

                    string to = user.Email;

                    string subject = "¡Compra Confirmada!";
                    

                    string body = BuildEmailBody(currentOrder);

                    await email.SendMessageAsync(to, subject, body, isHtml: true);
                }

                statusCode = Ok("Pedido registrado");
            }

            return statusCode;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ClearCart(string userId)
        {
            IActionResult statusCode;

            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                .FirstOrDefaultAsync(c => c.CartId.ToString().Equals(userId));

            if (cart == null)
            {
                statusCode = NotFound("Carrito no encontrado");
            }
            else
            {
                _dbContext.ProductsCart.RemoveRange(cart.ProductsCart);
                await _dbContext.SaveChangesAsync();

                statusCode = Ok("Carrito vaciado");
            }
            return statusCode;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        private string BuildEmailBody(Order order)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("<!DOCTYPE html>");
            stringBuilder.AppendLine("<html lang='es'>");
            stringBuilder.AppendLine("<head>");
            stringBuilder.AppendLine("<meta charset='UTF-8'>");
            stringBuilder.AppendLine("<title>Detalles del Pedido</title>");
            stringBuilder.AppendLine("<style>");
            stringBuilder.AppendLine("table { border-collapse: collapse; }");
            stringBuilder.AppendLine("th { background-color: rgb(214, 145, 124); text-align: center; }");
            stringBuilder.AppendLine("td { background-color: rgb(255, 239, 224); }");
            stringBuilder.AppendLine("td.numbers { text-align: right; }");
            stringBuilder.AppendLine("</style>");
            stringBuilder.AppendLine("</head>");
            stringBuilder.AppendLine("<body>");

            stringBuilder.AppendLine("<table border='1' cellpadding='10'>");
            stringBuilder.AppendLine("<tr><th colspan='4'>Detalles del pedido:</th></tr>");
            stringBuilder.AppendLine($"<tr><td>Número del pedido:</td><td class='numbers'>{order.OrderId}</td><td>Fecha del pedido:</td><td class='numbers'>{order.Date.ToString("dddd dd MMMM yyyy HH:mm zzz")}</td></tr>");
            stringBuilder.AppendLine("<tr><th colspan='4'>Productos:</th></tr>");
            stringBuilder.AppendLine("<tr><th>Nombre</th><th>Cantidad</th><th>Precio</th><th>Subtotal</th></tr>");

            foreach (var productOrder in order.ProductsOrders)
            {
                stringBuilder.AppendLine("<tr>");
                stringBuilder.AppendLine($"<td>{productOrder.Product.Name}</td>");
                stringBuilder.AppendLine($"<td class='numbers'>{productOrder.Amount}u</td>");
                stringBuilder.AppendLine($"<td class='numbers'>{productOrder.Product.EURprice}€</td>");
                stringBuilder.AppendLine($"<td class='numbers'>{(productOrder.Amount * productOrder.Product.EURprice)}€</td>");
                stringBuilder.AppendLine("</tr>");
            }

            stringBuilder.AppendLine("<tr><th colspan='4'>Total:</th></tr>");

            stringBuilder.AppendLine($"<tr><td colspan='3'>Total del pedido (eur):</td><td class='numbers'>{order.EURprice}€</td></tr>");
            stringBuilder.AppendLine($"<tr><td colspan='3'>Total del pedido (eth):</td><td class='numbers'>{order.ETHtotal}Ξ</td></tr>");

            stringBuilder.AppendLine("</table>");

            stringBuilder.AppendLine("</body>");
            stringBuilder.AppendLine("</html>");

            return stringBuilder.ToString();
        }
    }
}
