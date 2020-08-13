using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Todo.Domain.Api.Extensions
{
    public class AllowSameSiteAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;

            if (response != null)
            {
                //response.Cookies.Append("name", "value", new CookieOptions { SameSite = SameSiteMode.None, Secure = true });
                response.Headers.Add("Set-Cookie", "Secure; SameSite=None");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
