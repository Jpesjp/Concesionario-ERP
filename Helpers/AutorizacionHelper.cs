using Microsoft.AspNetCore.Mvc;

namespace ERPConcesionario.Helpers
{
    public static class AutorizacionHelper
    {
        public static IActionResult? ValidarSesionYRol(Controller controller, params string[] rolesPermitidos)
        {
            var username = controller.HttpContext.Session.GetString("Username");
            var rol = controller.HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(username))
                return controller.RedirectToAction("Login", "Account");

            if (rolesPermitidos != null && rolesPermitidos.Length > 0)
            {
                if (string.IsNullOrEmpty(rol) || !rolesPermitidos.Contains(rol))
                    return controller.RedirectToAction("AccesoDenegado", "Account");
            }

            return null;
        }
    }
}