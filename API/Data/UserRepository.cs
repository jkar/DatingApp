using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        public readonly DataContext _context;
        public readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            //1st way Manually kanw select ta pedia pou thelw sto select me vasi to MemberDto
            // return await _context.Users
            //     .Where(x => x.Username == username)
            //     .Select( user => new MemberDto
            //     {
            //        Id = user.Id,
            //        Username = user.Username
            //          .......
            //     } )
            //     .SingleOrDefaultAsync();

            //2nd way
            return await _context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        // public async Task<IEnumerable<MemberDto>> GetMembersAsync(UserParams userParams)
        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            // return await
            var query = _context.Users.AsQueryable();
                // .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                // .AsNoTracking()
                // .ToListAsync();

            //φερνει ολους τους χρηστες εκτος απο τον εαυτο του + gender (που απο το controller ζηταει το αντιθετο)
            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            //Περναει τα φιλτρα για ελαχιστη, μεγιστη ηλικια
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _=> query.OrderByDescending(u => u.LastActive)
            };

            //εδω γινεται execute to query Με pagintation με βαση τα pageNumber, pageSize apo ta query params
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking(), 
                                                        userParams.PageNumber, 
                                                        userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
            .Include(p => p.Photos)
            .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            //an swthike estw k ena stoixeio tha gyrisei ton arithmo twn allagwn sti vasi
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;

        }
    }
}