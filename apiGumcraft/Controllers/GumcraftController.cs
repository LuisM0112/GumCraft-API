using apiGumcraft.Database.Entities;
using GumcraftApi.Database;
using GumcraftApi.Models.Classes;
using GumcraftApi.Models.Database.Entities;
using GumcraftApi.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace apiGumcraft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GumcraftController : ControllerBase
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

                    statusCode = Ok("Usuario Registrado");
                }
                return statusCode;
            } catch (DbUpdateException ex)
            {
                ObjectResult statusCode;
                if (ex.InnerException == null)
                {
                    statusCode = BadRequest(ex.Message);
                } else
                {
                    SqliteException sqliteException = (SqliteException)ex.InnerException;
                    if (sqliteException.SqliteExtendedErrorCode == 2067)
                    {
                        statusCode = BadRequest("Usuario ya existente");
                    } else statusCode = BadRequest(sqliteException.Message);
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
    }
}

