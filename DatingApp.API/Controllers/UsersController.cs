using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

       
        // public async Task<IActionResult> GetUsers()   //*** modified to use paging
        // {
        //     var users = await _repo.GetUsers();
        //     var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
        //     return Ok(usersToReturn);
        // }
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams) // userParams will be coming from a query string
        {
           // for filtering
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(currentUserId);
            userParams.UserId = currentUserId;
            if (string.IsNullOrEmpty(userParams.Gender)) // if gender not specified
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }
           
            var users = await _repo.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            // we have access to the reponse header because we are inside the controller
            // users is a PagedList of users that contains the headers
            // we pass the page headers to teh client
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name ="GetUser")]  // L.131 added name to be used in CreatedAtRoute in Regiter method
        public async Task<IActionResult> GetUser(int id)
        {
                      
            var user = await _repo.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            // check if the user upding the profile is the logged in user
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            //get user from repo
            var userFromRepo = await _repo.GetUser(id);

            // write values from userForUpdateDto to userFRomRepo
            _mapper.Map(userForUpdateDto, userFromRepo);

            //persist
            if (await _repo.SaveAll())
                    return NoContent();

            // if somethign goes wrong
            throw new System.Exception($"Updating user {id} failed on save");

        }


    }
}