using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{   
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // api/users -> get's all the users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();

            return  Ok(users);
        }

        // api/users/lisa -> get's the user with username lisa
        [HttpGet("{username}", Name = "GetUser")] // we can use this route name in the AddPhoto() function
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        // api/users + 'memberDto' -> edits the user
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // Get the current user username using the token
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            // Automatically maps the memberUpdateDto to user, so now user has all the new informations that
            // needs to be updated
            _mapper.Map(memberUpdateDto, user);

            _userRepository.Update(user);

            if(await _userRepository.SaveAllAsync()) return NoContent();
            
            return BadRequest("Failed to update user");
        }

        // api/users/add-photo -> upload a photo for the current usser
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            // Get current user
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            // Upload the photo to cloudinary
            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            // Make an object containg the photoUrl received from cloudinary and the publicID
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            // If this is the only photo of the user, set it to main
            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            // Save the Photo obj to our DB
            user.Photos.Add(photo);
            
            if(await _userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
            }
                
            return BadRequest("Problem adding photo");
        }

        // api/users/set-main-photo/ + 'photoId' -> set the given photo as main
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo.IsMain) return BadRequest("This is already your main photo");

            // Make the current main photo not main anymore
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentMain != null) currentMain.IsMain = false;
            // Make the selected photo main
            photo.IsMain = true;

            if(await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failes to set main photo");
        }

        // api/users/delete-photo/ + 'photoId' -> delete the photo with the given id
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            // Get the current user
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            // Search the photo that needs to be deleted 
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null) return NotFound();

            // If the photo is main don't delete it
            if(photo.IsMain) return BadRequest("You cannot delete your main photo");

            // If the photo is saved in cloudinary delete the photo from there first
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                // If the delete from cloudinary fails then stop the execution of the task
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            // Delete the photo from the DB
            user.Photos.Remove(photo);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}