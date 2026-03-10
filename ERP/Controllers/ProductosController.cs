using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERP.Models;

namespace ERP.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IConfiguration _config;

        public ProductosController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            List<Producto> lista = new List<Producto>();
            string conn = _config.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(conn))
            {
                string query = "SELECT * FROM Productos";
                SqlCommand cmd = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Producto
                    {
                        IdProducto = (int)reader["IdProducto"],
                        Nombre = reader["Nombre"].ToString(),
                        Precio = (decimal)reader["Precio"],
                        Stock = (int)reader["Stock"]
                    });
                }
            }

            return View(lista);
        }
    }
}