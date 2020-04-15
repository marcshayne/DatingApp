using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _config = config;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("user name already exists");

            // *** removed and replaced by automapper intead
            // var userToCreate = new User
            // {
            //     UserName = userForRegisterDto.Username
            // };

            var userToCreate = _mapper.Map<User>(userForRegisterDto); // L.131 using auto mapper

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            // return StatusCode(201); // temporary

            // L.131  
            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);
            return CreatedAtRoute("GetUSer", new {controller = "Users", id = createdUser.Id},userToReturn);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            //throw new Exception("Computer says no");

            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            if (userFromRepo == null)
                return Unauthorized();

            //build up a token to return to the user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            //key to hash our token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            //generate sign-in credentials encryoted with hashing algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //save our token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds

            };

            // Token Handler
            var tokenHandler = new JwtSecurityTokenHandler();

            //create a token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // user information to be return to angular
            var user = _mapper.Map<UserForListDto>(userFromRepo); // Map userFromRepo into UserForListDto

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),

                //send additional user information outside the token to be stored in localstorage
                user
            });

        }

    }
}