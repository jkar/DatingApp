using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        public readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;

        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new 
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        //o admin borei na allaxei rolous s ena xristi me vasi tp username parameter tou xrisit
        //kai ta query params - roles opoy pairnaei auta k svinontai ola ta proigoumena
        //p.x ?roles=Member, Moderator
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            //pairnei apo ta params ta roles k ta kanei array me vasi to , poy uparxei anamesa se kathe role sto param
            var selectedRoles = roles.Split(",").ToArray();

            //vriksei ton user me vasi to username (parameter)
            var user = await _userManager.FindByNameAsync(username);

            //an de yparxei xristi stamatei i diadikasia me nmn
            if (user == null) return NotFound("Could not find user");

            //travaei tous rolous tou xrisit
            var userRoles = await _userManager.GetRolesAsync(user);

            //pernaei ta roles pou exei sta query params alla an uparxei idi o rolos sti vasi den to xanapernaei
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            //afairei olous tous rolous apo tin vasi ektos apo autous poy perase molis apo ta queryparams
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            // gyrnaei sto response tous updated roles tou user
            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or Moderators can see this");
        }
    }
}