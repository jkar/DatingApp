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

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseApiController
    {
        //public readonly DataContext _context;
        public readonly IUserRepository _userRepository;
        public readonly IMapper _mapper;

        //public UsersController(DataContext context)
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            //_context = context;
            _mapper = mapper;
            _userRepository = userRepository;
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
        [HttpGet("{username}")]
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
    }
}