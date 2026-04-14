using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERPConcesionario.Helpers;

namespace ERPConcesionario.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SqlConnection _connection;

        public DashboardController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras", "Ventas", "Inventario", "RRHH");
            if (acceso != null) return acceso;
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToAction("Login", "Account");


            ViewBag.TotalClientes = ObtenerConteo("SELECT COUNT(*) FROM Clientes");
            ViewBag.TotalProveedores = ObtenerConteo("SELECT COUNT(*) FROM Proveedores");
            ViewBag.TotalProductos = ObtenerConteo("SELECT COUNT(*) FROM Productos");
            ViewBag.TotalVehiculos = ObtenerConteo("SELECT COUNT(*) FROM VehiculosInventario");
            ViewBag.TotalEmpleados = ObtenerConteo("SELECT COUNT(*) FROM Empleados");
            ViewBag.TotalCompras = ObtenerConteo("SELECT COUNT(*) FROM OrdenesCompra");
            ViewBag.TotalVentas = ObtenerConteo("SELECT COUNT(*) FROM Ventas");
            ViewBag.TotalStockCritico = ObtenerConteo(@"
                SELECT COUNT(*)
                FROM InventarioProductos i
                INNER JOIN Productos p ON i.IdProducto = p.IdProducto
                WHERE i.StockActual <= p.StockMinimo");

            return View();
        }

        private int ObtenerConteo(string query)
        {
            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                int total = Convert.ToInt32(cmd.ExecuteScalar());
                _connection.Close();
                return total;
            }
        }
    }
}