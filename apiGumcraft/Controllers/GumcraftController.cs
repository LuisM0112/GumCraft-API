using GumcraftApi.Database;
using GumcraftApi.Models.Database;
using GumcraftApi.Models.Database.Entities;
using GumcraftApi.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

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

        [HttpPost("Login")]
        public async Task<IActionResult> PostLogin([FromForm] UserDto userDto)
        {
            if (userDto == null)
            {

            }



            return Ok("Sesión Iniciada");
        }

        [HttpPost("SignUp")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Post([FromForm] UserDto userDto)
        {
            try
            {
                User newUser = new User()
                {
                    Name = userDto.UserName,
                    Email = userDto.Email,
                    Password = userDto.Password,
                    Address = userDto.Address,
                };

                await _dbContext.Users.AddAsync(newUser);
                await _dbContext.SaveChangesAsync();

                return Ok("Usuario Registrado");
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
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

