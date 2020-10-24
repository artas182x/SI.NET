using System;
using Microsoft.AspNetCore.Mvc;

namespace demo.Controllers
{
    [ApiController]
    [Route("cache")]
    public class CacheController : Controller 
    {
        [HttpGet("get-time")]
        public string GetTime() {
            DateTime now = DateTime.Now;
            return now.ToString();
        }

        [HttpGet("get-time-manual")]
        [ResponseCache(Duration = 10, NoStore = false)]
        public string GetTimeManual() {
            DateTime now = DateTime.Now;
            return now.ToString();
        }

        [HttpGet("get-time-profile")]
        [ResponseCache(CacheProfileName = "si.net")]
        public string GetTimeProfile() {
            DateTime now = DateTime.Now;
            return now.ToString();
        }
    }
}