using Microsoft.AspNetCore.Mvc;
using ERPConcesionario.Helpers;

namespace ERPConcesionario.Controllers
{
    public class VentasController : Controller
    {
        public IActionResult Index()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Ventas");
            if (acceso != null) return acceso;
            return View();
        }
    }
}