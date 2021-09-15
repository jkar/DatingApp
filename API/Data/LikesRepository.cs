using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        public readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;

        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            //φερνει το row, αν υπάρχει, me βάσει τα Id (χρηστη , (Liked)χρηστη) 
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        //αναλογα το predicate ,αν ειναι liked - φέρνει τους χρήστες που έχει κάνει like ο χρήστης με βάση το id (likesParam.UserId) του
        //αν ειναι το predicate, likedBy, φέρνει τους χρήστες που έχουν κάνει like στον χρηστη με βαση το id (likesParam.UserId) του
        //στις παραμετρους , στο likeParams περνανε και τα PageSize, PageNumber που είναι για το pagination
        public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
        // public async Task<IEnumerable<LikeDTO>> GetUserLikes(string predicate, int userId)
        {
            //ετοιμάζει τα queries, για να μπορύν να γινουν πανω τους εξτρα ενεργειες
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            // οταν ψαχνει ποιους χρηστες εκανε like ο χρήστης. 
            if (likesParams.Predicate == "liked")
            {
                //οποτε where ο χρηστης ειναι sourceUser στο Likes table 
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                //φερνει τo relationship απο το Like table με τους likedUsers που τους εκανε like
                users = likes.Select(like => like.LikedUser);
            }
            //οταν ψαχνει ποιοι εκαναν like στον χρηστη
            if (likesParams.Predicate == "likedBy")
            {
                //οποτε where ο χρηστης ειναι LikedUser στο Likes table
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                //φερνει το relationship απο το Like table με τους SourceUser που τον έκαναν like
                users = likes.Select(like => like.SourceUser);
            }

            // kanei select gia na ferei ta fields tou kathe xristi stin morfi tou LikeDTO
            var likedUsers = users.Select(user => new LikeDTO
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.Id
            });

            // sto createAsync tou PagedList εκτελείται το query, και περνάμε και pageNumber, pageSize
            //για να φέρει τα αποτελέσματα με βαση το offset και το limit που ορίζεται απο τα pageNumber, pageSize
            //sto Response που φέρνει το endpoit έχει τις παρακατω πληροφορίες στα Headers
            //p.x Headers -> pagination : {"currentPage":1,"itemsPerPage":10,"totalItems":1,"totalPages":1}
            return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            //φέρνει τον χρήστη με βάση το id απο τον πίνακa User και με το include φέρνει και την λίστα με τους χρήστες που έχει κάνει ήδη Like.
            return await _context.Users
                    .Include(x => x.LikedUsers)
                    .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}