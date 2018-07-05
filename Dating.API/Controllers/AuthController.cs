using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dating.API.Data;
using Dating.API.DTOs;
using Dating.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dating.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepo;
        private readonly IConfiguration configuration;

        public AuthController(IAuthRepository _authRepo, IConfiguration _configuration)
        {
            authRepo = _authRepo;
            configuration = _configuration;
               
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody]UserForRegisterDTO userForRegisterDTO)
        {
            userForRegisterDTO.Username = userForRegisterDTO.Username.Trim().ToLower();

            if (await authRepo.UserExist(userForRegisterDTO.Username))
                ModelState.AddModelError("Username", "Este nombre de usuario ya ha sido tomado");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);            

            var userToCreate = new User
            {
                Username = userForRegisterDTO.Username
            };

            var createUser = authRepo.Register(userToCreate, userForRegisterDTO.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody]UserForLoginDTO userForLoginDTO)
        {
            var userFromRepo = await authRepo.Login(userForLoginDTO.Username.Trim().ToLower(), 
                                                    userForLoginDTO.Password);

            if (userFromRepo == null)
                return Unauthorized();

            //generando el token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration.GetSection("AppSettings:Token").Value);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                    new Claim(ClaimTypes.Name, userFromRepo.Username)
                }),

                Expires = DateTime.Now.AddDays(1),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok( new {tokenString});
        }
    }
}