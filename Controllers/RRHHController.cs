using Microsoft.AspNetCore.Mvc;
using ERPConcesionario.Helpers;


namespace ERPConcesionario.Controllers
{
    public class RRHHController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}