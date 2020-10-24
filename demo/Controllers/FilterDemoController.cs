using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace demo.Controllers
{

    [Route("/filters/[action]")]
    public class FilterDemoController : IActionFilter
    {

        [ApiExplorerSettings(IgnoreApi = true)]
        public void OnActionExecuted(ActionExecutedContext context)
        {
            System.Console.WriteLine("Po wykonaniu akcji");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void OnActionExecuting(ActionExecutingContext context)
        {
            System.Console.WriteLine("Przed wejściem do akcji");
        }


        public string Reksio() {
             System.Console.WriteLine("Wewnątrz akcji");
            return "rezultat";
        }
    }
}