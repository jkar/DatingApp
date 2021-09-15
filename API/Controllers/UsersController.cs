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
using API.Helpers;

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseApiController
    {
        public readonly IUserRepository _userRepository;
        public readonly IMapper _mapper;

        public readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _photoService = photoService;
        }

        //api/users
        // [Authorize(Roles = "Admin")]
        //φερνει τους χρηστες με βαση τα userParams (φιλτρα) που βρίσκονται στο query params
        //επισης στα query params εχει τα pageNumber, pageSize gia pagination
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams) 
        {
        //1st way
            //return await _context.Users.ToListAsync(); 
        //2nd way
            //var users = await _userRepository.GetUsersAsync();
            //var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
            //return Ok(usersToReturn);
        //3rd way

            //φερνει τον χρηστη που κανει το request με βαση το username tou που το βρισκει απο to Token-claims
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            //στα userParams ενημερωνει το username
            userParams.CurrentUsername = user.UserName;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                //για να ζητησει στα φιλτρα τ αντιθετο φυλλο, τραβαει το φυλλο του χρηστη
                userParams.Gender = user.Gender == "male" ? "female" : "male";
            }

            //gyrnaei tous xristes me vasi ta filtra kai me pagination me vasi ta pageSize ,pageNumber (query params)
            var users = await _userRepository.GetMembersAsync(userParams);

            //περναει στο response  headers με key pagination
            //kai value π.χ Headers -> pagination : {"currentPage":1,"itemsPerPage":10,"totalItems":1,"totalPages":1}
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        //api/users/3
        // [Authorize(Roles = "Member")]
        //me vasi to username fernei ton antistoixo user
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

        //βρισκει τον user me vasi to username pou τραβάει απο το token-claim, και τον κάνει update 
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO) 
        {
            //1st way this gives us the user's useranme from the token that the API uses to authenticate 
            //on POST the 2nd way
            // var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.GetUsername();

            // φερνει τον χρηστη με βαση το username
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

        //swzei tin photo sto cloudinary kai ta metadata (url, publicId) sto table Photo
        //vriskei to username klasika apo to token-claim k antistoixa meta ton user
        //an einai h prwti photo tou xristi ,thn kanei main
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto (IFormFile file)
        {
            //2nd way, (with ClaimsPrincipalExtensions) this gives us the user's useranme from the token that the API uses to authenticate 
            
            //το username το βρισκει απο to Token-claims
            var username = User.GetUsername();
            // φερνει τον χρηστη με βαση το username
            var user = await _userRepository.GetUserByUsernameAsync(username);

            //kanei upload tin photo sto cloudinary
            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) {
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            // check if this is the first photo to upload, if true set it to main
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            //κανει tracking sto table Photo ta stoixeia tis photografias (url , publicId) για να μπορει να την τραβάει απο το cloudinary μ αυτες τις πληροφορίες
            user.Photos.Add(photo);

            //twra kanei save ta stoixeia sto table
            if (await _userRepository.SaveAllAsync())
            {
                //return _mapper.Map<PhotoDto>(photo);
                //ΕΤΣΙ επιστρεφει 201 που είνα το statusCode για post και με το GetUser καλέι το [HttpGet("username")] για να φέρει τον χρήστη
                return CreatedAtRoute("GetUser", new { username =  user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        //βρισκει την photo με βαση to photoId, κανει false αυτη που ηταν main και θετει Main την phοto που θελει
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto (int photoId)
        {
            //(with ClaimsPrincipalExtensions) this gives us the user's useranme from the token that the API uses to authenticate -> User.GetUsername()
            //φερνει τον χρηστη που κανει το request με βαση το username tou που το βρισκει απο to Token-claims
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            //βρισκει την φωτο apo to relation me ton user me vasi to photoId poy einai stin parametro tiw methodou
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            //an einai idi main i photo stelenei mnm 
            if (photo.IsMain)
            {
                return BadRequest("This is already your main photo");
            }

            // vriskei tin photo pou einai main kai tin thetei na min einai
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }

            //thetei main tin photo pou thelei
            photo.IsMain = true;

            //kanei to save stin vasi
            if (await _userRepository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to set main photo");
        }

        //me vasi to photoId vriskei tin photo, k meta me vasi to publicId tin diagrafei apo to cloudinary
        //meta tin diagrafei k apo tin vasi (Photo table)
        //mono an den einai main diagrafetai
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto (int photoId)
        {
            //(with ClaimsPrincipalExtensions) this gives us the user's useranme from the token that the API uses to authenticate -> User.GetUsername()
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

             //βρισκει την φωτο apo to relation me ton user me vasi to photoId poy einai stin parametro tiw methodou
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) {
                return NotFound();
            }

            //an eina i main gurnaei mnm oti den epitrepetai na diagrafei
            if (photo.IsMain) {
                return BadRequest("You cannot delete your main photo");
            }

            //an exei publicId (pou shmainei oti exei perastei sto cloudinary), me vasi to publicId tin diagrafei apo to cloudinary
            if (photo.PublicId != null) {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }

            //kanei tracking k ti diagrafi apo to tin vasi (Photo table)
            user.Photos.Remove(photo);

            //swxei tin allagi stin vasi
            if (await _userRepository.SaveAllAsync())
                {
                    return Ok();
                }

            return BadRequest("Failed to delete the photo");
        }
    }
}