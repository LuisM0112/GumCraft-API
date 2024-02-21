﻿using GumCraft_API.Database;
using GumCraft_API.Database.Entities;
using GumCraft_API.Models.Classes;
using GumCraft_API.Models.Database.Entities;
using GumCraft_API.Models.Dto;
using GumCraft_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
            return _dbContext.Users.Select(ToDto);
        }

        [HttpPost("SignUp")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Post([FromForm] NewUser incomingNewUser)
        {
            try
            {
                ObjectResult statusCode;
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
                return statusCode;
            }
            catch (DbUpdateException ex)
            {
                ObjectResult statusCode;
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
                return statusCode;

            }
        }

        private UserDto ToDto(User user)
        {
            return new UserDto
            {
                UserName = user.Name,
                Email = user.Email,
                Address = user.Address,
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
                EURprice = product.EURprice,
                ETHprice = product.ETHprice
            };
        }

        //Devuelve Product
        [HttpGet("Product/{productId}")]
        public async Task<IActionResult> GetProducts(long productId)
        {
            ObjectResult statusCode;
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
        public async Task<IActionResult> GetProductsInCart(long cartId)
        {
            string cartID = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.CartId.ToString().Equals(cartID));

            if (cart == null)
            {
                return NotFound("Carrito no encontrado");
            }

            var productCartDto = cart.ProductsCart.Select(pc => new ProductCartDto
            {
                ProductId = pc.Product.ProductId,
                Name = pc.Product.Name,
                Amount = pc.Amount,
                Price = pc.Product.EURprice * pc.Amount
            }).ToList();

            return Ok(productCartDto);
        }

        [HttpGet("cart/{cartId}/total")]
        public async Task<IActionResult> GetCartTotal(long cartId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

            if (cart == null)
            {
                return NotFound("Carrito no encontrado");
            }

            var total = cart.ProductsCart.Sum(pc => pc.Product.EURprice * pc.Amount);

            return Ok(total);
        }

        [Authorize]
        [HttpPut("cart/product/{productId}")]
        public async Task<IActionResult> AddProductToCart(long cartId, long productId)
        {
            string cartID = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.CartId.ToString().Equals(cartID));

            if (cart == null)
            {
                return NotFound("Carrito no encontrado");
            }

            var product = await _dbContext.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound("Producto no encontrado");
            }

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



            await _dbContext.SaveChangesAsync();

            return Ok("Producto añadido al carrito");
        }


        [HttpPut("cart/productDel/{productId}")]
        public async Task<IActionResult> DelProductToCart(long cartId, long productId)
        {
            string cartID = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.CartId.ToString().Equals(cartID));

            if (cart == null)
            {
                return NotFound("Carrito no encontrado");
            }

            var product = await _dbContext.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound("Producto no encontrado");
            }

            var productCart = cart.ProductsCart.FirstOrDefault(pc => pc.Product.ProductId == productId);
            if (productCart == null)
            {
                return BadRequest("El producto no está en el carrito");
            }

            if (productCart.Amount > 1)
            {
                productCart.Amount--;
            }
            else
            {
                cart.ProductsCart.Remove(productCart);
            }

            await _dbContext.SaveChangesAsync();

            return Ok("Producto eliminado correctamente");
        }
        
        [HttpGet("GetUserById")]
        [Authorize]
        public ActionResult<string> GetUser()
        {
            var userId = User.FindFirst("id").Value;

            Console.WriteLine($"Reclamación de ID encontrada en el token: {userId}");

            if (!long.TryParse(userId, out long longId))
            {
                Console.WriteLine($"Formato de ID inválido: {userId}");
                return BadRequest("Invalid ID format");
            }

            Console.WriteLine($"ID de usuario parseado correctamente: {longId}");

            var user = _dbContext.Users.FirstOrDefault(u => u.UserId == longId);
            if (user == null)
            {
                Console.WriteLine($"No se encontró ningún usuario con el ID: {longId}");
                return NotFound();
            }

            Console.WriteLine($"Usuario encontrado: {user.UserId}");
            return user.Name;
        }

        [Authorize]
        [HttpPost("cart/clear")]
        public async Task<IActionResult> ClearCart()
        {
            string cartID = User.FindFirst("id").Value;
            var cart = await _dbContext.Carts
                .Include(c => c.ProductsCart)
                .FirstOrDefaultAsync(c => c.CartId.ToString().Equals(cartID));

            if (cart == null)
            {
                return NotFound("Carrito no encontrado");
            }

            _dbContext.ProductsCart.RemoveRange(cart.ProductsCart);
            await _dbContext.SaveChangesAsync();

            return Ok("Carrito vaciado");
        }


    }
}

