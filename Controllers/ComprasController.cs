using Microsoft.AspNetCore.Mvc;
using ERPConcesionario.Helpers;

namespace ERPConcesionario.Controllers
{
    public class ComprasController : Controller
    {
        public IActionResult Index()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;
            return View();
        }
    }
}