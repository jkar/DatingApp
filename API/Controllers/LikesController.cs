using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        public IUserRepository _userRepository;
        public ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
            _userRepository = userRepository;
        }

        //Ο χρήστης με βάση το id, username του απο το Token κανει Like σε άλλο χρήστη που έχει username τη παραμετρο username
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            // τραβάει το Id του Χρήστη που κάνει like απο τα Claims (μέσα στο Token)
            var sourceUserId = User.GetUserId();
            //βρισκει τον (liked)χρηστη που θα πάρει like με βαση το username του
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);
            //γυρνάει τον χρήστη που κάνει like με βαση το id του και έχει μέσα και την λίστα με τους LikedUsers
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            //αν δεν υπαρχει (liked)χρηστης σταματαει
            if (likedUser == null) return NotFound();

            //αν το username της παραμετρου είναι ίδιο με το username του χρηστη που κάνει like, του λεει οτι δεν γινετα να κανει  like τον εαυτό του
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            //Βλέπει στον πίνακα Likes αν υπάρχει εγγραφή με τα ids, αν υπάρχει σημαίνει ότι έγινε ήδη Like o (liked)user 
            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already like this user");

            //Δημιουργεί το row για τον πίνακα Likes
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            //Κάνει tracking στο LikedUsers array που τράβηξε παραπάνω την εγγραφή για το Like
            sourceUser.LikedUsers.Add(userLike);

            //Εδώ σώζεται και πρακτικά στην βάση
            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        //η παραμετρος LikesParams έχει αρχικά απο το query το predicate και μεσα στη method τραβαει απο το Token-Claims το userId
        //και τα pageNumber, pageSize που είναι για το pagination
        [HttpGet]
        // public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes(string predicate)
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            // τραβάει το Id του Χρήστη που κάνει like απο τα Claims (μέσα στο Token)
            likesParams.UserId = User.GetUserId();

            //το predicate ειναι queryparam (π.χ api/Likes?predicate=liked) (οπως και τα pageSize, pageNumber)
            //αναλογα το predicate ,αν ειναι liked - φέρνει τους χρήστες που έχει κάνει like ο χρήστης με βάση το id (likesParam.UserId) του
            //αν ειναι το predicate, likedBy, φέρνει τους χρήστες που έχουν κάνει like στον χρηστη με βαση το id (likesParam.UserId) του
            //epistrefei toys χρηστες kai ξανά γυρνάει τα count, pageNumber, pageSize gia το pagination στα headers sto key pagination
            var users = await _likesRepository.GetUserLikes(likesParams);
            // var users = await _likesRepository.GetUserLikes(predicate, User.GetUserId());

            //to AddPaginationHeader είναι στα extensions και φτιάχνει το key - pagination στα headers με value
            //Π.Χ //Headers -> pagination : {"currentPage":1,"itemsPerPage":10,"totalItems":1,"totalPages":1}
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}