using apiGumcraft.Database.Entities;
using GumcraftApi.Database;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("getusers")]
        public IEnumerable<User> GetUsers()
        {
            return _dbContext.Users;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return Ok("User created successfully");//no se si dejarlo en ingles
            }

            return BadRequest("Error");
        }
    }
}

