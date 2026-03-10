using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERP.Models;

namespace ERP.Controllers
{
    public class VentasController : Controller
    {
        private readonly IConfiguration _config;

        public VentasController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            List<Venta> lista = new List<Venta>();
            string conn = _config.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(conn))
            {
                string query = "SELECT * FROM Ventas";
                SqlCommand cmd = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Venta
                    {
                        IdVenta = (int)reader["IdVenta"],
                        IdCliente = (int)reader["IdCliente"],
                        Fecha = (DateTime)reader["Fecha"],
                        Total = (decimal)reader["Total"]
                    });
                }
            }

            return View(lista);
        }
    }
}