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

        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            return _dbContext.Users;
        }
    }
}

