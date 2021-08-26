using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUsername (this ClaimsPrincipal user)
        {
            //this gives us the user's useranme from the token that the API uses to authenticate 
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}