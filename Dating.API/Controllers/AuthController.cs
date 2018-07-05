using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dating.API.Data;
using Dating.API.DTOs;
using Dating.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dating.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepo;

        public AuthController(IAuthRepository _authRepo)
        {
            authRepo = _authRepo;
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

            var createUser = authRepo.Register(userToCreate, userForRegisterDTO.Username);

            return StatusCode(201);
        }
    }
}