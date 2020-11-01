using System;
using System.Web;
using Microsoft.AspNetCore.Rewrite;

namespace demo.Tools
{
 public class RewriteRules
{
    public static void RedirectRequests(RewriteContext context)
    {
            var referer = context.HttpContext.Request.Headers["Referer"];
            if (!Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(referer)) {
            var request = context.HttpContext.Request;
            var path = request.Path.Value;
   
            Uri refeferUri = new Uri(referer.ToString());
            string cultureParam = HttpUtility.ParseQueryString(refeferUri.Query).Get("culture");

            if (cultureParam != null && !request.QueryString.ToString().Contains("culture"))
            {
                var parametersToAdd = new System.Collections.Generic.Dictionary<string, string> { { "culture", cultureParam } };
                var newUri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(request.Path, parametersToAdd);
                context.HttpContext.Response.Redirect($"{ newUri }");
            }
        }
    }
}
}
