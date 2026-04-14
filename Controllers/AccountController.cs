using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERPConcesionario.Helpers;

namespace ERPConcesionario.Controllers
{
    public class AccountController : Controller
    {
        private readonly SqlConnection _connection;

        public AccountController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string query = @"
                SELECT TOP 1 u.IdUsuario, u.Username, r.NombreRol
                FROM Usuarios u
                INNER JOIN UsuarioRoles ur ON u.IdUsuario = ur.IdUsuario
                INNER JOIN Roles r ON ur.IdRol = r.IdRol
                WHERE u.Username = @Username
                  AND u.PasswordHash = @PasswordHash
                  AND u.Activo = 1";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Username", model.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", model.Password);

                _connection.Open();
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    HttpContext.Session.SetString("Username", reader["Username"].ToString() ?? "");
                    HttpContext.Session.SetString("Rol", reader["NombreRol"].ToString() ?? "");

                    _connection.Close();
                    return RedirectToAction("Index", "Dashboard");
                }

                _connection.Close();
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public static bool UsuarioLogueado(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Session.GetString("Username"));
        }

        public static string? ObtenerRol(HttpContext context)
        {
            return context.Session.GetString("Rol");
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}