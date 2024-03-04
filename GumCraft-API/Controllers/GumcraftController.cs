using GumCraft_API.Database;
using GumCraft_API.Models.Classes;
using GumCraft_API.Models.Database.Entities;
using GumCraft_API.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace GumCraft_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GumcraftController : Controller
    {
        private MyDbContext _dbContext;

        public GumcraftController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Devuelve user
        [HttpGet("Users")]
        public IEnumerable<UserDto> GetUsers()
        {
            var users = _dbContext.Users.ToList();
            return _dbContext.Users.Select(ToDto);
        }

        [HttpPost("SignUp")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Post([FromForm] NewUser incomingNewUser)
        {
            IActionResult statusCode;
            try
            {
                if (incomingNewUser.UserName == null || incomingNewUser.Email == null || incomingNewUser.Password == null || incomingNewUser.PasswordBis == null || incomingNewUser.Address == null)
                {
                    statusCode = BadRequest("Rellene los campos");
                }
                else if (incomingNewUser.Password != incomingNewUser.PasswordBis)
                {
                    statusCode = BadRequest("Las contraseñas no coinciden");
                }
                else
                {
                    User newUser = new User()
                    {
                        Name = incomingNewUser.UserName,
                        Email = incomingNewUser.Email,
                        Password = incomingNewUser.Password,
                        Address = incomingNewUser.Address,
                        Role = "USER"
                    };

                    await _dbContext.Users.AddAsync(newUser);
                    await _dbContext.SaveChangesAsync();
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == incomingNewUser.Email);
                    Cart newCart = new Cart()
                    {
                        User = user,
                        ProductsCart = new List<ProductCart>()
                    };

                    await _dbContext.Carts.AddAsync(newCart);
                    await _dbContext.SaveChangesAsync();


                    statusCode = Ok("Usuario registrado");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException == null)
                {
                    statusCode = BadRequest(ex.Message);
                }
                else
                {
                    SqliteException sqliteException = (SqliteException)ex.InnerException;
                    if (sqliteException.SqliteExtendedErrorCode == 2067)
                    {
                        statusCode = BadRequest("Usuario ya existente");
                    }
                    else statusCode = BadRequest(sqliteException.Message);
                }
            }
            return statusCode;
        }

        private UserDto ToDto(User user)
        {
            return new UserDto
            {
                UserName = user.Name,
                Email = user.Email,
                Address = user.Address,
                Role = user.Role,
                UserId = user.UserId
            };
        }

        //Devuelve Product
        [HttpGet("Products")]
        public IEnumerable<ProductDto> GetProducts()
        {
            return _dbContext.Products.Select(ToDto);
        }

        private ProductDto ToDto(Product product)
        {
            return new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Image = product.Image,
                Stock = product.Stock,
                EURprice = product.EURprice
            };
        }

        //Devuelve Product
        [HttpGet("Product/{productId}")]
        public async Task<IActionResult> GetProducts(long productId)
        {
            IActionResult statusCode;
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                statusCode = NotFound("Producto no encontrado");
            }
            else
            {
                statusCode = Ok(ToDto(product));
            }

            return statusCode;
        }

        [HttpGet("carts")]
        public IEnumerable<CartDto> GetCarts()
        {
            return _dbContext.Carts.Select(ToDto);
        }
        private CartDto ToDto(Cart cart)
        {
            return new CartDto
            {
                CartId = cart.CartId,
                UserId = cart.User.UserId
            };
        }

        [Authorize]
        [HttpGet("cart/products")]
        public async Task<IActionResult> GetProductsInCart()
        {
            IActionResult statusCode;

            string userId = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.User.UserId.ToString().Equals(userId));

            if (cart == null)
            {
                statusCode = NotFound("Carrito no encontrado");
            }
            else
            {
                var productCartDto = cart.ProductsCart.Select(pc => new ProductCartDto
                {
                    ProductId = pc.Product.ProductId,
                    Name = pc.Product.Name,
                    Amount = pc.Amount,
                    Price = pc.Product.EURprice * pc.Amount
                }).ToList();

                statusCode = Ok(productCartDto);
            }

            return statusCode;
        }

        [HttpGet("cart/total")]
        public async Task<IActionResult> GetCartTotal()
        {
            IActionResult statusCode;
            string userId = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.User.UserId.ToString().Equals(userId));

            if (cart == null)
            {
                statusCode = NotFound("Carrito no encontrado");
            }
            else
            {
                var total = cart.ProductsCart.Sum(pc => pc.Product.EURprice * pc.Amount);
                statusCode = Ok(total);
            }
            return statusCode;
        }

        [Authorize]
        [HttpPut("cart/product/{productId}")]
        public async Task<IActionResult> AddProductToCart(long productId)
        {
            IActionResult statusCode;

            string userId = User.FindFirst("id").Value;
            var cart = _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefault(c => c.User.UserId.ToString().Equals(userId));

            var product = _dbContext.Products.Find(productId);

            if (cart == null)
            {
                statusCode = NotFound("Carrito no encontrado");
            }
            else if (product == null)
            {
                statusCode = NotFound("Producto no encontrado");
            }
            else
            {
                var productCart = cart.ProductsCart.FirstOrDefault(pc => pc.Product.ProductId == productId);

                if (productCart != null)
                {
                    productCart.Amount++;
                }
                else
                {

                    productCart = new ProductCart
                    {
                        Cart = cart,
                        Product = product,
                        Amount = 1
                    };

                    cart.ProductsCart.Add(productCart);
                }
                _dbContext.SaveChanges();
                statusCode = Ok("Producto añadido al carrito");
            }

            return statusCode;
        }

        [HttpPut("cart/productDel/{productId}")]
        public async Task<IActionResult> DelProductToCart(long productId)
        {
            IActionResult statusCode;

            string userId = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.User.UserId.ToString().Equals(userId));

            var product = await _dbContext.Products.FindAsync(productId);

            if (cart == null)
            {
                statusCode = NotFound("Carrito no encontrado");
            }
            else if (product == null)
            {
                statusCode = NotFound("Producto no encontrado");
            }
            else
            {
                var productCart = cart.ProductsCart.FirstOrDefault(pc => pc.Product.ProductId == productId);
                if (productCart == null)
                {
                    statusCode = BadRequest("El producto no está en el carrito");
                }
                else
                {
                    if (productCart.Amount > 1)
                    {
                        productCart.Amount--;
                    }
                    else
                    {
                        cart.ProductsCart.Remove(productCart);
                    }

                    await _dbContext.SaveChangesAsync();

                    statusCode = Ok("Producto eliminado correctamente");
                }
            }
            return statusCode;
        }
        
        [HttpGet("GetUserById")]
        [Authorize]
        public IActionResult GetUser()
        {
            IActionResult statusCode;

            var userId = User.FindFirst("id").Value;

            var user = _dbContext.Users.FirstOrDefault(u => u.UserId.ToString().Equals(userId));
            if (user == null)
            {
                statusCode = NotFound("Usuario no encontrado");
            }
            else
            {
                statusCode = Ok(ToDto(user));
            }
            return statusCode;
        }

        [HttpDelete("deleteUser/{userId}")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult DeleteUser(long userId)
        {
            IActionResult statusCode;

            var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                statusCode = NotFound();
            }
            else
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
                statusCode = Ok("Usuario borrado con exito");
            }
            return statusCode;
        }

        [Authorize]
        [HttpPost("cart/clear")]
        public async Task<IActionResult> ClearCart()
        {
            IActionResult statusCode;

            string userId = User.FindFirst("id").Value;
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

        [Authorize (Roles = "ADMIN")]
        [HttpGet("imAdmin")]
        public async Task<bool> imAdmin() 
        { 
            return true;
        }

        [HttpPut("changeRole/{userId}")]
        [Authorize (Roles = "ADMIN")]
        public IActionResult ChangeUserRole(long userId)
        {
            IActionResult statusCode;

            var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                statusCode = NotFound();
            }
            else
            {
                if (user.Role == "ADMIN")
                {
                    user.Role = "USER";
                }
                else if (user.Role == "USER")
                {
                    user.Role = "ADMIN";
                }

                _dbContext.SaveChanges();

                statusCode = Ok("El rol del usuario ha sido cambiado a "+user.Role);
            }
            return statusCode;
        }

        //Devuelve Order
        [HttpGet("Orders")]
        public async Task<IActionResult> GetOrders()
        {
            IActionResult statusCode;
            string userId = User.FindFirst("id").Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId.ToString().Equals(userId));
            if (user == null)
            {
                statusCode = NotFound("Usuario no encontrado");
            }
            else
            {
                var orders = await _dbContext.Orders
                .Include(o => o.ProductsOrders)
                    .ThenInclude(po => po.Product)
                .Where(o => o.User.UserId.ToString().Equals(userId))
                .ToListAsync();
                if (orders == null || !orders.Any())
                {
                    statusCode = NotFound("No hay pedidos");
                }
                else
                {
                    var orderDTOs = orders.Select(ToDto).ToList();
                    statusCode = Ok(orderDTOs);
                }
            }
            return statusCode;
        }

        private OrderDto ToDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                Status = order.Status,
                Date = order.Date.ToString("dddd dd MMMM yyyy HH:mm zzz"),
                EURprice = order.EURprice,
                ETHtotal = order.ETHtotal
            };
        }
    }
}
