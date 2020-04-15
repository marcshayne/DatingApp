using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, 
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }
        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult>  GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, 
                [FromForm]PhotoForCreationDto photoForCreationDto)
        {
           // check if the userid from teh token matches the userid in the route
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            //get user from repo
            var userFromRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;

            // create a variable to store the results we get back from Cloudinary
            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using (var stream = file.OpenReadStream()) //read our uploaded file into memory
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);                    
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            // map to Photo from PhotoForCreationDto 
            var photo = _mapper.Map<Photo>(photoForCreationDto);

            // check if there is no photo as IsMain
            if(!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;

            userFromRepo.Photos.Add(photo); 

            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
               
                // Post should return CreatedAtRoute with 3rd overload (string routeName, object routeValues, object value)
                return CreatedAtRoute("GetPhoto", new {userId = userId, id = photo.Id}, photoToReturn);

            }

            return BadRequest("Could not add the photo");

        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
             // check if the userid from teh token matches the userid in the route
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            //get user from repo
            var user = await _repo.GetUser(userId);

            //check if the photo is part of the user's photo collection
            if (!user.Photos.Any(p => p.Id ==id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if(photoFromRepo.IsMain)
                return BadRequest("This is already the main photo");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("Counld not set photo to main");
        }

        [HttpDelete("{id}")]   // L.121
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            // check if the userid from teh token matches the userid in the route
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            //get user from repo
            var user = await _repo.GetUser(userId);

            //check if the photo is part of the user's photo collection
            if (!user.Photos.Any(p => p.Id ==id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if(photoFromRepo.IsMain)
                return BadRequest("You are not allowed to deleted main photo");

            //delete from Cloudinary
            if(photoFromRepo.PublicId != null){  // is a Cloudinary Photo
                  var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);
                if (result.Result == "ok") {
                    _repo.Delete(photoFromRepo);
                } 
            }

            if(photoFromRepo.PublicId == null){ // not a cloudinary photo
                 _repo.Delete(photoFromRepo);
            }
         
            if(await _repo.SaveAll())
                return Ok();
            
            return BadRequest("Failed to delete the photo");
        }
    }
}