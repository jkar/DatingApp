using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using API.Data;
using API.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using API.Extensions;
using System;

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseApiController
    {
        //public readonly DataContext _context;
        public readonly IUserRepository _userRepository;
        public readonly IMapper _mapper;

        public readonly IPhotoService _photoService;

        //public UsersController(DataContext context)
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            //_context = context;
            _mapper = mapper;
            _userRepository = userRepository;
            _photoService = photoService;
        }

        //    api/users
        [HttpGet]
        //[AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers() 
        {
        //1st way
            //return await _context.Users.ToListAsync(); 
        //2nd way    
            //var users = await _userRepository.GetUsersAsync();
            //var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
            //return Ok(usersToReturn);
        //3rd way
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }

        //    api/users/3
        //[Authorize]
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) 
        {
        //1st way
            //return await _context.Users.FindAsync(id);
        //2nd way
            //var user = await _userRepository.GetUserByUsernameAsync(username);
            //var userToReturn = _mapper.Map<MemberDto>(user);
            //return Ok(userToReturn);
        //3rd way
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO) 
        {
            //1st way this gives us the user's useranme from the token that the API uses to authenticate 
            //on POST the 2nd way
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _userRepository.GetUserByUsernameAsync(username);
            //source ->  to
            _mapper.Map(memberUpdateDTO, user);
            //alliws manually p.x
            //user.City = memberUpdateDTO.City;
            //.....

            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto (IFormFile file)
        {
            //2nd way, (with ClaimsPrincipalExtensions) this gives us the user's useranme from the token that the API uses to authenticate 
            var username = User.GetUsername();
            var user = await _userRepository.GetUserByUsernameAsync(username);

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) {
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
                //PublicId = Int16.Parse(result.PublicId)
            };

            // check if this is the first photo to upload, if true set it to main
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
            {
                //return _mapper.Map<PhotoDto>(photo);
                //ΕΤΣΙ επιστρεφει 201 που είνα το statusCode για post και με το GetUser καλέι το [HttpGet("username")] για να φέρει τον χρήστη
                return CreatedAtRoute("GetUser", new { username =  user.Username }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto (int photoId)
        {
            //(with ClaimsPrincipalExtensions) this gives us the user's useranme from the token that the API uses to authenticate -> User.GetUsername()
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain)
            {
                return BadRequest("This is already your main photo");
            }

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }

            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to set main photo");
        }
    }
}