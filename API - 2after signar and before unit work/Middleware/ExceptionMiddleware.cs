using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using API.Errors;
using System.Text.Json;

namespace API.Middleware
{

    //ΔΕΝ ΧΡΗΣΙΜΟΠΟΙΩ TRY-CATCH BLOCKS STOUS CONTROLLERS, ΘΑ ΠΕΡΑΣΩ ΣΑΝ MIDDLEWARE ΑΥΤΗ ΤΗΝ CLASS Κ ΟΤΑΝ ΧΤΥΠΑΕΙ EXCEPTION ΣΤΟΥΣ CONTROLLERS
    //ΘΑ ΑΝΕΒΑΙΝΕΙ ΠΑΝΩ ΕΝΑ ΕΝΑ ΣΤΑ MIDDLEWARES ΜΕΧΡΙ ΝΑ ΒΡΕΙ CATCH ΠΟΥ ΤΑ ΠΙΑΝΕΙ, Κ ΕΧΩ ΕΔΩ Σ ΑΥΤΟ ΤΟ MIDDLEWRARE ΠΟΥ ΤΟ CATCH ΠΙΑΝΕΙ ΤΑ 
    //ERROR EXCEPTIONS.
    //BY DEFAULT, ME TO NEXT APLA PERNAEI TO HTTP REQUEST STA ΕΠΟΜΕΝΑ MIDDLEWARES
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try {
                await _next(context);
            } catch (Exception ex) {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                //ewrthmatiko sto stackrace se periptwsi pou den yparxei
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiException(context.Response.StatusCode, "Internal Server Error");

                    var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
                    var json = JsonSerializer.Serialize(response, options);
                    await context.Response.WriteAsync(json);
            }
        }
    }
}