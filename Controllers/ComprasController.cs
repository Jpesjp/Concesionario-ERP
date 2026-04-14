using Microsoft.AspNetCore.Mvc;

namespace ERPConcesionario.Controllers
{
    public class ComprasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}