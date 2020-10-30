using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace demo.Controllers
{
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


        [HttpGet("set-time-distributed-sql")]
        public string SetTimeWithIDistributedCache([FromServices] IDistributedCache cache) {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(5));
            options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(20));
            DateTime time = DateTime.Now;
            cache.Set("time", Encoding.ASCII.GetBytes(time.ToString()), options);
            return "Ustawiłem wartość cache";
        }

        [HttpGet("get-time-distributed-sql")]
        public string GetTimeWithIDistributedCache([FromServices] IDistributedCache cache) {
            byte[] tab = cache.Get("time");
            if(tab == null) { return "Nie udało się pobać wartości time";} 

            // Mala modyfikacja:
            // Zwrocenie nazwy hosta, aby bylo wiadomo skad przyszła odpowiedz
            return "Host: " + Environment.MachineName + "\n" + Encoding.ASCII.GetString(tab);
        }

        [HttpGet("set-temp")]
        public string SetTempValue() {
            this.TempData["mojklucz"] = "moja wartość";
            return "Ustawiłem zmienną tymczasową";
        }

        [HttpGet("get-temp")]
        public string GetTempValue() {
            if(!this.TempData.ContainsKey("mojklucz")) { return "brak ustawionego klucza"; }
            String value = (String)this.TempData["mojklucz"];
            return value;
        }

        [HttpGet("set-session")]
        public string SetSessionVar() {
            HttpContext.Session.Set("mojklucz2", Encoding.ASCII.GetBytes("pewna wartość"));
            return "Ustawiłem wartość sesyjną";
        }

        [HttpGet("get-session")]
        public string GetSessionVar() {
            byte[] resp = null;
            HttpContext.Session.TryGetValue("mojklucz2", out resp);
            if(resp == null) {
                return "Nie znalazłem klucza";
            }
            return Encoding.ASCII.GetString(resp);
        }
    }
}