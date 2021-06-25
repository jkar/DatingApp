using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;
        public BuggyController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            //δεν θα βρει τπτ γιατι δεν υπαρχει κατι στη θεση -1
            var thing = _context.Users.Find(-1);
            if (thing == null) {
                return NotFound();
            }

            return Ok(thing);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            // try {
                            //δεν θα βρει τπτ γιατι δεν υπαρχει κατι στη θεση -1
            var thing = _context.Users.Find(-1);

            // Θα δωσει error γιατι ειναι Null και το null δε γινεται string, θα δώσει error - nullReferenceException
            var thingToReturn = thing.ToString();
            return thingToReturn;
            // } catch (Exception ex) {
            //     return StatusCode(500, "Server says no!");
            // }
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("this was not a good request");
        }

    }
}