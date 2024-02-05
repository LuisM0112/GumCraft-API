using GumcraftApi.Models.Classes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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
        private readonly TokenValidationParameters _tokenParameters;

        public AuthController(IOptionsMonitor<JwtBearerOptions> jwtOptions)
        {
            _tokenParameters = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme)
                .TokenValidationParameters;
        }
        [HttpPost("loginModel")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            try
            {
                //Si el usuario existe entonces creamos y le damos su token
                if (model.Username == "Angel" && model.Password == "12345")
                {

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        //Añadimos los datos que sirvan para autorizar al usuario
                        Claims = new Dictionary<string, object>
                    {
                        { "id", Guid.NewGuid().ToString() },
                        { ClaimTypes.Role, "admin" }
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
                //Si el usuario no existe, lo indicamos
                return Unauthorized("El usuario no existe");
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
