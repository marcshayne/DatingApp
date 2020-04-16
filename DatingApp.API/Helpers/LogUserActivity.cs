using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // to get the http context
            var resultContext = await next();

            // get teh userid from token
            var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // get repo
            var repo = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();
            // get user from repo
            var user  = await repo.GetUser(userId);
            // update lastactivedate
            user.LastActive = DateTime.Now;
            // save
            await repo.SaveAll();

        }
    }
}