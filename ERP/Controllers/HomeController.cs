using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ERP.Controllers
{
    public class HomeController : Controller
    {
        private readonly SqlConnection _connection;

        public HomeController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Dashboard()
        {
            int clientes = 0;
            int productos = 0;
            int ventas = 0;

            _connection.Open();

            SqlCommand c1 = new SqlCommand("SELECT COUNT(*) FROM Clientes", _connection);
            clientes = (int)c1.ExecuteScalar();

            SqlCommand c2 = new SqlCommand("SELECT COUNT(*) FROM Productos", _connection);
            productos = (int)c2.ExecuteScalar();

            SqlCommand c3 = new SqlCommand("SELECT COUNT(*) FROM Ventas", _connection);
            ventas = (int)c3.ExecuteScalar();

            _connection.Close();

            ViewBag.Clientes = clientes;
            ViewBag.Productos = productos;
            ViewBag.Ventas = ventas;

            return View();
        }
    }
}