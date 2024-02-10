﻿using GumcraftApi.Database;
using GumcraftApi.Models.Classes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GumcraftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private MyDbContext _dbContext;

        private readonly TokenValidationParameters _tokenParameters;

        public AuthController(IOptionsMonitor<JwtBearerOptions> jwtOptions, MyDbContext dbContext)
        {
            _tokenParameters = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme)
                .TokenValidationParameters;
            _dbContext = dbContext;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] LoggedUser incomingLoggedUser)
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
                    try
                    {
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            //Añadimos los datos que sirvan para autorizar al usuario
                            Claims = new Dictionary<string, object>
                            {
                                { "id", Guid.NewGuid().ToString() },
                                { ClaimTypes.Role, ""+user.Role+"" }
                            },
                            //Añadimos la fecha de caducidad 
                            Expires = DateTime.UtcNow.AddYears(5),
                            //Aquí especificamos nuestra clave y el algoritmo de firmado 
                            SigningCredentials = new SigningCredentials(
                                _tokenParameters.IssuerSigningKey,
                                SecurityAlgorithms.HmacSha256Signature
                                )
                        };
                        //Creamos el token y se lo devolvemos al usuario logeado
                        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                        string stringToken = tokenHandler.WriteToken(token);

                        return Ok(stringToken);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }
            return statusCode;
        }
    }
}
