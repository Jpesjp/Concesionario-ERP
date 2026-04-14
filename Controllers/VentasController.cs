using Microsoft.AspNetCore.Mvc;

namespace ERPConcesionario.Controllers
{
    public class VentasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}