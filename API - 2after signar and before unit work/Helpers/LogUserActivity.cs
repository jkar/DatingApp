using System;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    //m auto to filter pou to pernaw sta applicationServiceExtensions, kanw update to last Active, an einai authenticated o user me tin twrino date,time.
    //to pernaw epishs sto BaseController pou to kanoun implement oloi oi controllers
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            //Checks if user is not authenticated
            if (!resultContext.HttpContext.User.Identity.IsAuthenticated)
            {
                return ;
            }
            //to GetUsername() to travaw apo ta extensions gia na mou dwsei t username apo to token
            var userId = resultContext.HttpContext.User.GetUserId();
            var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
            var user = await repo.GetUserByIdAsync(userId);
            user.LastActive = DateTime.Now;
            await repo.SaveAllAsync();
        }
    }
}