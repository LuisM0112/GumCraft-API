using GumcraftApi.Database;
using GumcraftApi.Models.Classes;
using GumcraftApi.Models.Database.Entities;
using GumcraftApi.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
        [HttpGet]
        public IEnumerable<UserDto> GetUsers()
        {
            return _dbContext.Users.Select(ToDto);
        }

        [HttpGet("GetUserByEmail")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }
            else
            {
                return Ok(ToDto(user));
            }
        }

        [HttpPost("Login")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostLogin([FromForm] LoggedUser incomingLoggedUser)
        {
            ObjectResult statusCode;
            if (incomingLoggedUser.Email == null || incomingLoggedUser.Password == null)
            {
                statusCode = BadRequest("Rellene los campos");
            }
            else
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == incomingLoggedUser.Email);
                if (user == null)
                {
                    statusCode = BadRequest("El usuario no existe");
                }
                else if (user.Password != incomingLoggedUser.Password)
                {
                    statusCode = BadRequest("Contraseña equivocada");
                }
                else
                {
                    statusCode = Ok("Sesión Iniciada");
                }
            }
            return statusCode;
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

        private UserDto ToDto (User user)
        {
            return new UserDto
            {
                UserName = user.Name,
                Email = user.Email,
                Address = user.Address,
                Password = user.Password,
            };
        }
    }
}

